using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Shared.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Shared.Application.Generator.GeneratorDiagnostics;

namespace Shared.Application.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private const string Namespace = "Shared.Application.Generator";
    //private const string HandlerAttributeFullName = $"{Namespace}.{nameof(HandlerAttribute)}";

    // private static CompilationUnitSyntax HandlerAttribute()
    // {
    //     var attributeDecl = ClassDeclaration(nameof(HandlerAttribute))
    //         .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
    //         .AddBaseListTypes(SimpleBaseType(IdentifierName("Attribute")))
    //         .AddAttributeLists(
    //             AttributeList([
    //                 Attribute(IdentifierName("AttributeUsage"))
    //                     .AddArgumentListArguments(
    //                         AttributeArgument(IdentifierName("AttributeTargets").Member("Class")))
    //             ])
    //         )
    //         .WithoutBody();
    //
    //     var namespaceDecl = FileScopedNamespaceDeclaration(ParseName(Namespace))
    //         .AddMembers(attributeDecl);
    //
    //     var compilationUnit = CompilationUnit()
    //         .AddMembers(namespaceDecl)
    //         .ApplyGeneratorDefaults();
    //
    //     return compilationUnit;
    // }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // context.RegisterPostInitializationOutput(static postInitializationContext =>
        // {
        //     var attribute = HandlerAttribute();
        //     postInitializationContext.AddSource(HandlerAttributeFullName + ".g.cs",
        //         SourceText.From(attribute.ToFullString(), Encoding.UTF8));
        // });
        //

        var wellKnownTypesProvider = context.CompilationProvider
            .Select(static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return new WellKnownTypes(compilation);
            })
            .WithComparer(WellKnownTypesComparer.Instance);


        var candidateDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;
                    if (ctx.SemanticModel.GetDeclaredSymbol(classDecl, ct) is not { } symbol) return null;
                    return symbol.IsAbstract ? null : symbol;
                }
            )
            .Where(s => s is not null)
            .Select((s, _) => s!);


        var combined = candidateDeclarations.Combine(wellKnownTypesProvider);

        var handlers = combined.Select((pair, _) =>
            {
                var typeSymbol = pair.Left;
                var wk = pair.Right;

                foreach (var def in typeSymbol.AllInterfaces.Select(symbol => symbol.OriginalDefinition))
                {
                    if (wk.QueryHandler is not null &&
                        SymbolEqualityComparer.Default.Equals(def, wk.QueryHandler))
                    {
                        return typeSymbol;
                    }

                    if (wk.CommandHandler is not null &&
                        SymbolEqualityComparer.Default.Equals(def, wk.CommandHandler))
                    {
                        return typeSymbol;
                    }
                }

                return null;
            })
            .Where(s => s is not null)
            .Collect();
        
        var compilationHandlersPair = context.CompilationProvider.Combine(handlers);

        context.RegisterSourceOutput(compilationHandlersPair,
            static (spc, source) =>
            {
                try
                {
                    var (compilation, handlerSymbols) = source;
                    Execute(compilation, handlerSymbols!, spc);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(InternalError, null, exception.Message));
                }
            });
    }

    private static void Execute(Compilation compilation, ImmutableArray<INamedTypeSymbol> handlersSymbol,
        SourceProductionContext context)
    {
        var services = IdentifierName("services");
        List<StatementSyntax> statements = [];
        foreach (var handlerSymbol in handlersSymbol)
        {
            if (!handlerSymbol.IsGenericType)
            {
                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                            services.GenericMember("AddScoped", handlerSymbol.GetGlobalName())
                        )
                    )
                );
            }
            else
            {
                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                                services.Member("AddScoped")
                            )
                            .AddArgumentListArguments(
                                Argument(TypeOfExpression(handlerSymbol.ConstructUnboundGenericType().GetGlobalName()))
                            )
                    )
                );
            }
        }

        statements.Add(
            ReturnStatement(IdentifierName("services"))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        );

        var methodDecl = MethodDeclaration(ParseTypeName("IServiceCollection"), "RegisterHandlers")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("services")).AddModifiers(Token(SyntaxKind.ThisKeyword))
                    .WithType(ParseTypeName("IServiceCollection"))
            )
            .WithBody(Block(statements));

        var classDecl = ClassDeclaration("Mediator")
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
            .AddMembers(methodDecl);

        var compilationUnit = CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("Microsoft.Extensions.DependencyInjection"))
            )
            .AddMembers(
                FileScopedNamespaceDeclaration(ParseName(compilation.AssemblyName))
                    .AddMembers(classDecl)
            )
            .ApplyGeneratorDefaults();

        context.AddSource($"{compilation.AssemblyName}.Mediator.g.cs",
            SourceText.From(compilationUnit.ToFullString(), Encoding.UTF8));
    }
}