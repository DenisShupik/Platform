using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Shared.Presentation.Generator.Diagnostics;

namespace Shared.Presentation.Generator;

public sealed partial class Generator
{
    private const string GenerateBindAttributeMetadataName =
        "Shared.Presentation.Generator.Attributes.GenerateBindAttribute";
    private const string IncludeAttributeMetadataName =
        "Shared.TypeGenerator.Attributes.IncludeAttribute";
    private const string OmitAttributeMetadataName =
        "Shared.TypeGenerator.Attributes.OmitAttribute";
    private const string ResultNamespace = "Shared.Domain.Abstractions.Results";
    private const string ResultMarkerMetadataName = "IResult";
    private const string ResultContractMetadataName = "IResult`1";
    private const string SuccessMetadataName = "Shared.Domain.Abstractions.Success";
    private const string UserIdMetadataName = "Shared.Domain.ValueObjects.UserId";
    private const string ActorContextMetadataName = "Shared.Domain.ValueObjects.ActorContext";
    private const string PaginationLimitMetadataName = "Shared.Application.ValueObjects.PaginationLimit";

    private static readonly SymbolDisplayFormat FullyQualifiedTypeFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private void InitializeEndpoints(IncrementalGeneratorInitializationContext context)
    {
        var endpointTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsGeneratedEndpointRegistration(node),
                static (syntaxContext, cancellationToken) =>
                    GetEndpointTarget(syntaxContext, cancellationToken))
            .Where(static target => target is not null)
            .Select(static (target, _) => target!)
            .Collect();

        var documentationFiles = context.AdditionalTextsProvider
            .Where(static file => string.Equals(
                Path.GetFileName(file.Path),
                "Api.en.xml",
                StringComparison.OrdinalIgnoreCase))
            .Select(static (file, cancellationToken) => new DocumentationFile(
                file.Path,
                file.GetText(cancellationToken)?.ToString() ?? string.Empty))
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(endpointTargets).Combine(documentationFiles),
            static (sourceContext, input) =>
            {
                try
                {
                    ExecuteEndpoints(
                        sourceContext,
                        input.Left.Left,
                        input.Left.Right,
                        input.Right);
                }
                catch (Exception exception)
                {
                    sourceContext.ReportDiagnostic(Diagnostic.Create(
                        InternalError,
                        null,
                        exception.ToString()));
                }
            });
    }

    private static bool IsGeneratedEndpointRegistration(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name: GenericNameSyntax genericName
                }
            })
            return false;

        var argumentCount = genericName.TypeArgumentList.Arguments.Count;
        return genericName.Identifier.ValueText switch
        {
            "MapGet" or "MapPost" or "MapPut" or "MapPatch" or "MapDelete" => argumentCount == 2,
            "MapPostCreatedAt" => argumentCount == 3,
            _ => false
        };
    }

    private static EndpointTarget? GetEndpointTarget(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
        var genericName = (GenericNameSyntax)memberAccess.Name;
        if (invocation.ArgumentList.Arguments.Count != 1)
            return null;

        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
        if (receiverType is not INamedTypeSymbol namedReceiver ||
            !IsOrImplements(namedReceiver, "Microsoft.AspNetCore.Routing.IEndpointRouteBuilder"))
            return null;

        var containingDeclaration = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (containingDeclaration is null ||
            context.SemanticModel.GetDeclaredSymbol(containingDeclaration, cancellationToken) is not INamedTypeSymbol apiType)
            return null;

        var typeArguments = genericName.TypeArgumentList.Arguments;
        if (context.SemanticModel.GetTypeInfo(typeArguments[0], cancellationToken).Type is not INamedTypeSymbol requestType ||
            context.SemanticModel.GetTypeInfo(typeArguments[1], cancellationToken).Type is not INamedTypeSymbol handlerType)
            return null;

        INamedTypeSymbol? createdAtType = null;
        if (typeArguments.Count == 3)
        {
            if (context.SemanticModel.GetTypeInfo(typeArguments[2], cancellationToken).Type is not
                INamedTypeSymbol resolvedCreatedAtType)
                return null;
            createdAtType = resolvedCreatedAtType;
        }

        var routeExpression = invocation.ArgumentList.Arguments[0].Expression;
        if (!TryGetRoutePattern(
                routeExpression,
                context.SemanticModel,
                cancellationToken,
                out var endpointPattern))
            return null;

        var routeParameters = TryGetCompleteRouteParameters(
            memberAccess.Expression,
            routeExpression,
            context.SemanticModel,
            cancellationToken);

        return new EndpointTarget(
            apiType,
            new EndpointSpecification(requestType, handlerType, createdAtType),
            genericName.Identifier.ValueText == "MapPostCreatedAt"
                ? "MapPost"
                : genericName.Identifier.ValueText,
            routeExpression.GetLocation(),
            typeArguments[1].GetLocation(),
            typeArguments.Count == 3 ? typeArguments[2].GetLocation() : null,
            routeParameters,
            endpointPattern,
            namedReceiver.ToDisplayString() == "Microsoft.AspNetCore.Routing.RouteGroupBuilder");
    }

    private static ImmutableArray<RouteParameterPattern> TryGetCompleteRouteParameters(
        ExpressionSyntax receiver,
        ExpressionSyntax endpointPattern,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (!TryGetRoutePattern(endpointPattern, semanticModel, cancellationToken, out var endpointRoute))
            return default;

        var routePatterns = new List<string>();
        var visitedLocals = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        if (!TryCollectRouteGroupPatterns(
                receiver,
                semanticModel,
                cancellationToken,
                visitedLocals,
                routePatterns))
            return default;

        routePatterns.Add(endpointRoute);
        var routeParameters = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var routePattern in routePatterns)
            if (!TryExtractRouteParameters(routePattern, routeParameters))
                return default;

        return routeParameters
            .Select(parameter => new RouteParameterPattern(parameter.Key, parameter.Value))
            .ToImmutableArray();
    }

    private static bool TryCollectRouteGroupPatterns(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<ISymbol> visitedLocals,
        ICollection<string> routePatterns)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        if (expression is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax memberAccess
            } invocation)
        {
            if (!TryCollectRouteGroupPatterns(
                    memberAccess.Expression,
                    semanticModel,
                    cancellationToken,
                    visitedLocals,
                    routePatterns))
                return false;

            if (GetSimpleName(memberAccess.Name) != "MapGroup")
                return true;

            return invocation.ArgumentList.Arguments.Count > 0 &&
                   TryGetRoutePattern(
                       invocation.ArgumentList.Arguments[0].Expression,
                       semanticModel,
                       cancellationToken,
                       out var groupPattern) &&
                   AddRoutePattern(routePatterns, groupPattern);
        }

        if (expression is IdentifierNameSyntax identifier &&
            semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol is ILocalSymbol local)
        {
            if (!visitedLocals.Add(local)) return false;
            var declarator = local.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(cancellationToken))
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault();
            return declarator?.Initializer is not null &&
                   TryCollectRouteGroupPatterns(
                       declarator.Initializer.Value,
                       semanticModel,
                       cancellationToken,
                       visitedLocals,
                       routePatterns);
        }

        return true;
    }

    private static bool AddRoutePattern(ICollection<string> routePatterns, string pattern)
    {
        routePatterns.Add(pattern);
        return true;
    }

    private static string GetSimpleName(SimpleNameSyntax name) => name.Identifier.ValueText;

    private static bool TryGetRoutePattern(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        out string pattern)
    {
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constant is { HasValue: true, Value: string value })
        {
            pattern = value;
            return true;
        }

        if (semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol is IFieldSymbol
            {
                Name: "Empty",
                ContainingType.SpecialType: SpecialType.System_String
            })
        {
            pattern = string.Empty;
            return true;
        }

        pattern = string.Empty;
        return false;
    }

    private static bool TryExtractRouteParameterNames(
        string pattern,
        ISet<string> parameterNames)
    {
        var parameters = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        if (!TryExtractRouteParameters(pattern, parameters)) return false;
        foreach (var parameter in parameters.Keys) parameterNames.Add(parameter);
        return true;
    }

    private static bool TryExtractRouteParameters(
        string pattern,
        IDictionary<string, bool> parameters)
    {
        for (var index = 0; index < pattern.Length; index++)
        {
            if (pattern[index] != '{') continue;
            if (index + 1 < pattern.Length && pattern[index + 1] == '{')
            {
                index++;
                continue;
            }

            var contentStart = index + 1;
            var contentEnd = -1;
            for (var current = contentStart; current < pattern.Length; current++)
            {
                if (current + 1 < pattern.Length &&
                    (pattern[current] == '{' && pattern[current + 1] == '{' ||
                     pattern[current] == '}' && pattern[current + 1] == '}'))
                {
                    current++;
                    continue;
                }

                if (pattern[current] != '}') continue;
                contentEnd = current;
                break;
            }

            if (contentEnd < 0) return false;

            var content = pattern.Substring(contentStart, contentEnd - contentStart).TrimStart('*');
            var delimiter = content.IndexOfAny(new[] { ':', '=', '?' });
            var name = (delimiter < 0 ? content : content.Substring(0, delimiter)).Trim();
            if (name.Length == 0) return false;
            var isOptional = content.EndsWith("?", StringComparison.Ordinal) ||
                             content.IndexOf('=') >= 0;
            parameters[name] = parameters.TryGetValue(name, out var alreadyOptional)
                ? alreadyOptional || isOptional
                : isOptional;
            index = contentEnd;
        }

        return true;
    }

    private static bool IsOrImplements(INamedTypeSymbol type, string metadataName) =>
        type.ToDisplayString() == metadataName ||
        type.AllInterfaces.Any(candidate => candidate.ToDisplayString() == metadataName);

    private static string GenerateEndpointRegistration(
        INamedTypeSymbol apiType,
        EndpointSpecification specification,
        string mapMethod,
        string endpointMethod)
    {
        var requestType = specification.RequestType;
        var accessibility = requestType.DeclaredAccessibility switch
        {
            Accessibility.Public => "public ",
            Accessibility.Internal => "internal ",
            _ => string.Empty
        };
        var source = new StringBuilder()
            .AppendLine("// <auto-generated />")
            .AppendLine("#nullable enable");

        if (!requestType.ContainingNamespace.IsGlobalNamespace)
            source.Append("namespace ")
                .Append(requestType.ContainingNamespace.ToDisplayString())
                .AppendLine(";")
                .AppendLine();

        source.Append(accessibility);
        if (requestType.IsSealed) source.Append("sealed ");
        source.Append("partial class ")
            .Append(requestType.Name)
            .Append(" : global::Shared.Presentation.Abstractions.IGeneratedEndpoint<")
            .Append(TypeName(requestType))
            .Append(", ")
            .Append(TypeName(specification.HandlerType))
            .AppendLine(">")
            .AppendLine("{")
            .AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Shared.Presentation.Generator\", \"1.0.0\")]")
            .AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]")
            .AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]")
            .AppendLine("    static global::Microsoft.AspNetCore.Builder.RouteHandlerBuilder global::Shared.Presentation.Abstractions.IGeneratedEndpoint<")
            .Append("        ")
            .Append(TypeName(requestType))
            .Append(", ")
            .Append(TypeName(specification.HandlerType))
            .AppendLine(">.Map(")
            .AppendLine("        global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints,")
            .AppendLine("        string pattern) =>")
            .AppendLine("        global::Microsoft.AspNetCore.Builder.RoutingEndpointConventionBuilderExtensions.WithName(")
            .Append("            global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.")
            .Append(mapMethod)
            .Append("(endpoints, pattern, global::")
            .Append(apiType.ToDisplayString())
            .Append('.')
            .Append(endpointMethod)
            .AppendLine("),")
            .Append("            nameof(global::")
            .Append(apiType.ToDisplayString())
            .Append('.')
            .Append(endpointMethod)
            .AppendLine("));")
            .AppendLine("}");

        return source.ToString();
    }

    private static void ExecuteEndpoints(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<EndpointTarget> targets,
        ImmutableArray<DocumentationFile> documentationFiles)
    {
        if (targets.IsDefaultOrEmpty) return;

        var documentation = ReadDocumentation(context, documentationFiles);
        var usedDocumentationKeys = new HashSet<string>(StringComparer.Ordinal);

        var generatedRequestRegistrations = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var duplicateEndpointTargets = new HashSet<EndpointTarget>();
        foreach (var duplicateGroup in targets.GroupBy(
                     target => GetEndpointMethodName(target.Specification.RequestType),
                     StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            foreach (var duplicateTarget in duplicateGroup)
            {
                duplicateEndpointTargets.Add(duplicateTarget);
                context.ReportDiagnostic(Diagnostic.Create(
                    DuplicateEndpointName,
                    duplicateTarget.Location,
                    duplicateGroup.Key));
            }
        }

        foreach (var group in targets.GroupBy(
                     candidate => candidate.ApiType.ToDisplayString(),
                     StringComparer.Ordinal))
        {
            var apiType = group.First().ApiType;
            var methods = new StringBuilder();

            foreach (var target in group)
            {
                var specification = target.Specification;
                var methodName = GetEndpointMethodName(specification.RequestType);
                var operationKey = LowerFirst(methodName.Substring(0, methodName.Length - "Async".Length));
                usedDocumentationKeys.Add(operationKey);

                ValidateRouteParameters(context, target, methodName);
                ValidateRouteConvention(context, target, methodName);

                if (duplicateEndpointTargets.Contains(target))
                    continue;

                if (!TryGetHandlerContract(
                        context,
                        target,
                        methodName,
                        out var handlerContract))
                    continue;

                if (!ValidateHandlerConvention(context, target, methodName, handlerContract))
                    continue;

                if (!TryResolveCreatedAtType(
                        context,
                        target,
                        targets,
                        handlerContract.ResultType,
                        methodName,
                        out var createdAtType))
                    continue;

                if (!generatedRequestRegistrations.Add(specification.RequestType))
                {
                    ReportInvalidEndpoint(
                        context,
                        target.Location,
                        methodName,
                        "the request type is already registered by another generated endpoint");
                    continue;
                }

                if (!documentation.TryGetValue(operationKey, out var documentationEntry))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        EndpointDocumentationMissing,
                        target.Location,
                        properties: ImmutableDictionary<string, string?>.Empty
                            .Add("OperationKey", operationKey)
                            .Add("DocumentationPath", documentationFiles.FirstOrDefault()?.Path),
                        messageArgs: new object[] { operationKey }));
                    documentationEntry = new DocumentationEntry(operationKey, Location.None);
                }

                if (!TryGenerateEndpointMethod(
                        context,
                        compilation,
                        apiType,
                        specification,
                        handlerContract,
                        createdAtType,
                        target.HandlerLocation,
                        methodName,
                        documentationEntry.Summary,
                        out var methodSource))
                    continue;

                methods.Append(methodSource);

                context.AddSource(
                    $"{specification.RequestType.ToDisplayString().Replace('.', '_')}.EndpointRegistration.g.cs",
                    SourceText.From(
                        GenerateEndpointRegistration(apiType, specification, target.MapMethod, methodName),
                        Encoding.UTF8));
            }

            if (methods.Length == 0) continue;

            var source = new StringBuilder()
                .AppendLine("// <auto-generated />")
                .AppendLine("#nullable enable")
                .Append("namespace ")
                .Append(apiType.ContainingNamespace.ToDisplayString())
                .AppendLine(";")
                .AppendLine()
                .Append("public static partial class ")
                .Append(apiType.Name)
                .AppendLine()
                .AppendLine("{")
                .Append(methods)
                .AppendLine("}")
                .ToString();

            context.AddSource(
                $"{apiType.ToDisplayString().Replace('.', '_')}.Endpoints.g.cs",
                SourceText.From(source, Encoding.UTF8));
        }

        foreach (var entry in documentation.Where(entry => !usedDocumentationKeys.Contains(entry.Key)))
            context.ReportDiagnostic(Diagnostic.Create(
                EndpointDocumentationUnused,
                entry.Value.Location,
                entry.Key));
    }

    private static void ValidateRouteParameters(
        SourceProductionContext context,
        EndpointTarget target,
        string endpointName)
    {
        if (target.RouteParameters.IsDefault) return;

        var requestType = target.Specification.RequestType;
        var routeProperties = GetEffectiveProperties(requestType)
            .Where(property => property.Symbol.GetAttributes().Any(attribute =>
                attribute.AttributeClass?.ToDisplayString() ==
                "Microsoft.AspNetCore.Mvc.FromRouteAttribute"))
            .Select(property => new RouteProperty(
                property.Name,
                GetBindingName(property.Symbol) ?? LowerFirst(property.Name)))
            .ToArray();

        var unmatchedRouteParameters = target.RouteParameters
            .Where(parameter => routeProperties.All(property => !string.Equals(
                property.BindingName,
                parameter.Name,
                StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var unmatchedRouteProperties = routeProperties
            .Where(property => target.RouteParameters.All(parameter => !string.Equals(
                parameter.Name,
                property.BindingName,
                StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var endpointRouteParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasEndpointRouteParameters = TryExtractRouteParameterNames(
            target.EndpointPattern,
            endpointRouteParameters);

        foreach (var routeParameter in unmatchedRouteParameters)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RouteParameterNotBound,
                target.Location,
                properties: ImmutableDictionary<string, string?>.Empty
                    .Add("RouteParameter", routeParameter.Name)
                    .Add(
                        "SuggestedRouteParameter",
                        unmatchedRouteParameters.Length == 1 &&
                        unmatchedRouteProperties.Length == 1 &&
                        hasEndpointRouteParameters &&
                        endpointRouteParameters.Contains(routeParameter.Name)
                            ? unmatchedRouteProperties[0].BindingName
                            : null),
                messageArgs: new object[]
                {
                    endpointName,
                    routeParameter.Name,
                    requestType.ToDisplayString()
                }));
        }

        foreach (var routeProperty in unmatchedRouteProperties)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                FromRoutePropertyNotInRoute,
                target.Location,
                properties: ImmutableDictionary<string, string?>.Empty.Add(
                    "SuggestedPattern",
                    AppendRouteParameter(target.EndpointPattern, routeProperty.BindingName)),
                messageArgs: new object[]
                {
                    endpointName,
                    routeProperty.PropertyName
                }));
        }

        foreach (var optionalParameter in target.RouteParameters.Where(parameter => parameter.IsOptional))
        {
            var routeProperty = routeProperties.FirstOrDefault(property => string.Equals(
                property.BindingName,
                optionalParameter.Name,
                StringComparison.OrdinalIgnoreCase));
            if (routeProperty is null) continue;

            context.ReportDiagnostic(Diagnostic.Create(
                OptionalRouteParameterIsNotSupported,
                target.Location,
                endpointName,
                optionalParameter.Name,
                routeProperty.PropertyName));
        }
    }

    private static Dictionary<string, DocumentationEntry> ReadDocumentation(
        SourceProductionContext context,
        ImmutableArray<DocumentationFile> files)
    {
        var result = new Dictionary<string, DocumentationEntry>(StringComparer.Ordinal);
        if (files.IsDefaultOrEmpty)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                EndpointDocumentationInvalid,
                null,
                "Documentation/Api.en.xml is not configured as an AdditionalFile"));
            return result;
        }

        foreach (var file in files)
        {
            try
            {
                var document = XDocument.Parse(file.Content, LoadOptions.SetLineInfo);
                foreach (var operation in document.Root?.Elements("operation") ?? Enumerable.Empty<XElement>())
                {
                    var operationLocation = GetXmlLocation(file, operation);
                    var key = operation.Attribute("key")?.Value;
                    var summary = operation.Element("summary")?.Value.Trim();
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(summary))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            EndpointDocumentationInvalid,
                            operationLocation,
                            $"{file.Path} contains an operation without a key or summary"));
                        continue;
                    }

                    if (result.ContainsKey(key!))
                        context.ReportDiagnostic(Diagnostic.Create(
                            EndpointDocumentationDuplicate,
                            operationLocation,
                            key));
                    else
                        result.Add(key!, new DocumentationEntry(summary!, operationLocation));
                }
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    EndpointDocumentationInvalid,
                    null,
                    $"{file.Path}: {exception.Message}"));
            }
        }

        return result;
    }

    private static Location GetXmlLocation(DocumentationFile file, XObject node)
    {
        if (!(node is IXmlLineInfo lineInfo) || !lineInfo.HasLineInfo())
            return Location.None;

        var text = SourceText.From(file.Content, Encoding.UTF8);
        var lineIndex = lineInfo.LineNumber - 1;
        if (lineIndex < 0 || lineIndex >= text.Lines.Count)
            return Location.None;

        var line = text.Lines[lineIndex];
        var start = Math.Min(line.End, line.Start + Math.Max(0, lineInfo.LinePosition - 1));
        var span = TextSpan.FromBounds(start, line.End);
        return Location.Create(file.Path, span, text.Lines.GetLinePositionSpan(span));
    }

    private static bool TryGetHandlerContract(
        SourceProductionContext context,
        EndpointTarget target,
        string methodName,
        out HandlerContract contract)
    {
        var contracts = target.Specification.HandlerType.AllInterfaces
            .Where(candidate =>
                candidate.ContainingNamespace.ToDisplayString() == "Shared.Application.Interfaces" &&
                candidate is { TypeArguments.Length: 2 } &&
                candidate.MetadataName is "IQueryHandler`2" or "ICommandHandler`2")
            .ToArray();

        if (contracts.Length != 1 ||
            contracts[0].TypeArguments[0] is not INamedTypeSymbol applicationRequestType)
        {
            ReportInvalidEndpoint(
                context,
                target.HandlerLocation,
                methodName,
                $"handler '{target.Specification.HandlerType.ToDisplayString()}' must implement exactly one ICommandHandler<TRequest, TResult> or IQueryHandler<TRequest, TResult>");
            contract = null!;
            return false;
        }

        var handlerInterface = contracts[0];
        contract = new HandlerContract(
            applicationRequestType,
            handlerInterface.TypeArguments[1],
            handlerInterface.MetadataName == "IQueryHandler`2"
                ? HandlerKind.Query
                : HandlerKind.Command);
        return true;
    }

    private static bool ValidateHandlerConvention(
        SourceProductionContext context,
        EndpointTarget target,
        string methodName,
        HandlerContract contract)
    {
        var expectedKind = target.MapMethod == "MapGet" ? HandlerKind.Query : HandlerKind.Command;
        if (contract.Kind != expectedKind)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HandlerKindDoesNotMatchHttpMethod,
                target.HandlerLocation,
                methodName,
                target.MapMethod.Substring("Map".Length).ToUpperInvariant(),
                expectedKind.ToString().ToLowerInvariant(),
                target.Specification.HandlerType.ToDisplayString(),
                contract.Kind.ToString().ToLowerInvariant()));
            return false;
        }

        var requestStem = RemoveSuffix(target.Specification.RequestType.Name, "Request");
        var applicationStem = RemoveSuffix(
            RemoveSuffix(contract.RequestType.Name, "Query"),
            "Command");
        var handlerStem = RemoveSuffix(
            RemoveSuffix(target.Specification.HandlerType.Name, "QueryHandler"),
            "CommandHandler");
        if (!string.Equals(requestStem, applicationStem, StringComparison.Ordinal) ||
            !string.Equals(requestStem, handlerStem, StringComparison.Ordinal))
            context.ReportDiagnostic(Diagnostic.Create(
                EndpointNamingConventionMismatch,
                target.HandlerLocation,
                target.Specification.RequestType.Name,
                contract.RequestType.Name,
                target.Specification.HandlerType.Name));

        return true;
    }

    private static void ValidateRouteConvention(
        SourceProductionContext context,
        EndpointTarget target,
        string methodName)
    {
        if (!target.IsRouteGroup || !target.EndpointPattern.StartsWith("/", StringComparison.Ordinal))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            GroupRouteStartsWithSlash,
            target.Location,
            properties: ImmutableDictionary<string, string?>.Empty.Add(
                "SuggestedPattern",
                target.EndpointPattern.TrimStart('/')),
            messageArgs: new object[] { methodName }));
    }

    private static bool TryResolveCreatedAtType(
        SourceProductionContext context,
        EndpointTarget target,
        ImmutableArray<EndpointTarget> targets,
        ITypeSymbol applicationResultType,
        string methodName,
        out INamedTypeSymbol? createdAtType)
    {
        var valueType = GetSuccessfulValueType(applicationResultType);
        createdAtType = target.Specification.CreatedAtType;

        if (createdAtType is not null)
        {
            var explicitCreatedAtType = createdAtType;
            if (valueType is not null &&
                targets.Any(candidate =>
                    candidate.MapMethod == "MapGet" &&
                    SymbolEqualityComparer.Default.Equals(candidate.Specification.RequestType, explicitCreatedAtType) &&
                    TryGetCreatedAtRouteProperty(explicitCreatedAtType, valueType, out _)))
                return true;

            context.ReportDiagnostic(Diagnostic.Create(
                InvalidCreatedAtTarget,
                target.CreatedAtLocation ?? target.Location,
                methodName,
                createdAtType.ToDisplayString(),
                valueType?.ToDisplayString() ?? applicationResultType.ToDisplayString()));
            return false;
        }

        if (target.MapMethod != "MapPost" ||
            !target.Specification.RequestType.Name.StartsWith("Create", StringComparison.Ordinal) ||
            valueType is null ||
            IsType(valueType, SuccessMetadataName))
            return true;

        var candidates = targets
            .Where(candidate =>
                candidate.MapMethod == "MapGet" &&
                TryGetCreatedAtRouteProperty(candidate.Specification.RequestType, valueType, out _))
            .ToArray();
        var canonicalRequestName = valueType.Name.EndsWith("Id", StringComparison.Ordinal)
            ? "Get" + valueType.Name.Substring(0, valueType.Name.Length - "Id".Length) + "Request"
            : string.Empty;
        var canonicalCandidates = candidates
            .Where(candidate => candidate.Specification.RequestType.Name == canonicalRequestName)
            .ToArray();

        if (canonicalCandidates.Length == 1)
        {
            createdAtType = canonicalCandidates[0].Specification.RequestType;
            return true;
        }

        if (candidates.Length == 1)
        {
            createdAtType = candidates[0].Specification.RequestType;
            return true;
        }

        var reason = candidates.Length == 0
            ? "no matching GET endpoint is registered"
            : "matching candidates are " + string.Join(
                ", ",
                candidates.Select(candidate => candidate.Specification.RequestType.ToDisplayString()));
        context.ReportDiagnostic(Diagnostic.Create(
            CreatedAtTargetCannotBeInferred,
            target.Location,
            methodName,
            valueType.ToDisplayString(),
            reason));
        return false;
    }

    private static ITypeSymbol? GetSuccessfulValueType(ITypeSymbol applicationResultType)
    {
        if (IsSuccessOr(applicationResultType)) return null;

        var resultContract = GetResultContract(applicationResultType);
        if (resultContract is not null) return resultContract.TypeArguments[0];

        return applicationResultType;
    }

    private static bool TryGetCreatedAtRouteProperty(
        INamedTypeSymbol requestType,
        ITypeSymbol valueType,
        out RouteProperty routeProperty)
    {
        var routeProperties = GetEffectiveProperties(requestType)
            .Where(property => property.Symbol.GetAttributes().Any(attribute =>
                attribute.AttributeClass?.ToDisplayString() ==
                "Microsoft.AspNetCore.Mvc.FromRouteAttribute"))
            .ToArray();
        if (routeProperties.Length != 1 ||
            !SymbolEqualityComparer.Default.Equals(routeProperties[0].Type, valueType))
        {
            routeProperty = null!;
            return false;
        }

        routeProperty = new RouteProperty(
            routeProperties[0].Name,
            GetBindingName(routeProperties[0].Symbol) ?? LowerFirst(routeProperties[0].Name));
        return true;
    }

    private static bool TryGenerateEndpointMethod(
        SourceProductionContext context,
        Compilation compilation,
        INamedTypeSymbol apiType,
        EndpointSpecification specification,
        HandlerContract handlerContract,
        INamedTypeSymbol? createdAtType,
        Location handlerLocation,
        string methodName,
        string summary,
        out string source)
    {
        source = string.Empty;
        var applicationRequestType = handlerContract.RequestType;
        var applicationResultType = handlerContract.ResultType;
        var requestProperties = GetEffectiveProperties(specification.RequestType);
        var applicationProperties = GetEffectiveProperties(applicationRequestType);
        var body = requestProperties.FirstOrDefault(property => property.Name == "Body");
        var bodyProperties = body?.Type is INamedTypeSymbol bodyType
            ? GetEffectiveProperties(bodyType)
            : ImmutableArray<EffectiveProperty>.Empty;
        var authorization = GetAuthorizationMode(specification.RequestType);

        var assignments = new List<string>();
        var mappingFailed = false;
        foreach (var targetProperty in applicationProperties)
        {
            if (!TryMapApplicationProperty(
                    compilation,
                    targetProperty,
                    requestProperties,
                    bodyProperties,
                    authorization,
                    out var expression))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    EndpointPropertyCannotBeMapped,
                    handlerLocation,
                    methodName,
                    targetProperty.Name,
                    specification.RequestType.ToDisplayString()));
                mappingFailed = true;
                continue;
            }

            assignments.Add($"                {targetProperty.Name} = {expression}");
        }

        if (mappingFailed) return false;

        if (!TryBuildHttpResult(
                context,
                apiType,
                methodName,
                applicationResultType,
                createdAtType,
                out var responseType,
                out var responseStatements))
            return false;

        var summaryLiteral = SyntaxFactory.Literal(summary).ToString();
        var builder = new StringBuilder();
        builder.AppendLine("    /// <summary>");
        foreach (var line in summary.Replace("\r", string.Empty).Split('\n'))
            builder.Append("    /// ").Append(EscapeXml(line.Trim())).AppendLine();
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Shared.Presentation.Generator\", \"1.0.0\")]");
        builder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]");
        builder.Append("    [global::Microsoft.AspNetCore.Http.EndpointSummaryAttribute(")
            .Append(summaryLiteral)
            .AppendLine(")] ");
        builder.Append("    public static async global::System.Threading.Tasks.Task<")
            .Append(responseType)
            .Append("> ")
            .Append(methodName)
            .AppendLine("(");
        builder.Append("        ")
            .Append(TypeName(specification.RequestType))
            .AppendLine(" request,");
        builder.Append("        [global::Microsoft.AspNetCore.Mvc.FromServicesAttribute] ")
            .Append(TypeName(specification.HandlerType))
            .AppendLine(" handler,");
        builder.AppendLine("        global::System.Threading.CancellationToken cancellationToken)");
        builder.AppendLine("    {");
        builder.Append("        var result = await handler.HandleAsync(new ")
            .Append(TypeName(applicationRequestType))
            .AppendLine();
        builder.AppendLine("        {");
        builder.AppendLine(string.Join(",\n", assignments));
        builder.AppendLine();
        builder.AppendLine("        }, cancellationToken);");
        builder.AppendLine();
        builder.Append(responseStatements);
        builder.AppendLine("    }");
        builder.AppendLine();

        source = builder.ToString();
        return true;
    }

    private static bool TryMapApplicationProperty(
        Compilation compilation,
        EffectiveProperty target,
        ImmutableArray<EffectiveProperty> requestProperties,
        ImmutableArray<EffectiveProperty> bodyProperties,
        AuthorizationMode authorization,
        out string expression)
    {
        var requestProperty = requestProperties.FirstOrDefault(property => property.Name == target.Name);
        if (requestProperty is not null)
        {
            expression = ConvertExpression(
                compilation,
                target.Type,
                requestProperty.Type,
                "request." + requestProperty.Name);
            return expression.Length > 0;
        }

        var bodyProperty = bodyProperties.FirstOrDefault(property => property.Name == target.Name);
        if (bodyProperty is not null)
        {
            expression = ConvertExpression(
                compilation,
                target.Type,
                bodyProperty.Type,
                "request.Body." + bodyProperty.Name);
            return expression.Length > 0;
        }

        if (target.Name.EndsWith("At", StringComparison.Ordinal) &&
            IsType(target.Type, "System.DateTime"))
        {
            expression = "global::System.DateTime.UtcNow";
            return true;
        }

        if (authorization != AuthorizationMode.None)
        {
            if (IsType(target.Type, ActorContextMetadataName) ||
                IsNullableOf(target.Type, ActorContextMetadataName))
            {
                expression = "request.RequestedBy";
                return true;
            }

            if (IsType(target.Type, UserIdMetadataName) &&
                (target.Name == "UserId" ||
                 target.Name == "QueriedBy" ||
                 target.Name == "RequestedBy" ||
                 target.Name.EndsWith("By", StringComparison.Ordinal)))
            {
                expression = "request.RequestedBy.UserId";
                return true;
            }

        }

        expression = string.Empty;
        return false;
    }

    private static string ConvertExpression(
        Compilation compilation,
        ITypeSymbol targetType,
        ITypeSymbol sourceType,
        string sourceExpression)
    {
        if (IsType(targetType, PaginationLimitMetadataName) &&
            !IsType(sourceType, PaginationLimitMetadataName))
            return $"{TypeName(targetType)}.From({sourceExpression}.Value)";

        if (IsType(sourceType, ActorContextMetadataName) && IsType(targetType, UserIdMetadataName))
            return sourceExpression + ".UserId";

        var conversion = compilation.ClassifyConversion(sourceType, targetType);
        return conversion.Exists && (conversion.IsIdentity || conversion.IsImplicit)
            ? sourceExpression
            : string.Empty;
    }

    private static bool TryBuildHttpResult(
        SourceProductionContext context,
        INamedTypeSymbol apiType,
        string methodName,
        ITypeSymbol applicationResultType,
        INamedTypeSymbol? createdAtType,
        out string responseType,
        out string responseStatements)
    {
        var branches = new List<HttpResultBranch>();
        var errorTypes = new List<ITypeSymbol>();
        INamedTypeSymbol? domainResult = null;
        var isSuccessOr = false;

        if (applicationResultType is INamedTypeSymbol namedResult && IsSuccessOr(namedResult))
        {
            domainResult = namedResult;
            isSuccessOr = true;

            if (createdAtType is not null)
            {
                ReportInvalidEndpoint(
                    context,
                    apiType,
                    methodName,
                    "CreatedAt requires an application result value");
                responseType = string.Empty;
                responseStatements = string.Empty;
                return false;
            }

            branches.Add(HttpResultBranch.NoContent());
            foreach (var errorType in namedResult.TypeArguments)
            {
                if (!TryCreateErrorBranch(errorType, out var errorBranch))
                {
                    ReportInvalidEndpoint(
                        context,
                        apiType,
                        methodName,
                        $"error type '{errorType.ToDisplayString()}' does not derive from a supported HTTP error base type");
                    responseType = string.Empty;
                    responseStatements = string.Empty;
                    return false;
                }

                branches.Add(errorBranch);
                errorTypes.Add(errorType);
            }
        }
        else if (applicationResultType is INamedTypeSymbol valueResult &&
                 GetResultContract(valueResult) is { } resultContract)
        {
            domainResult = valueResult;
            if (!TryCreateSuccessBranch(
                    context,
                    apiType,
                    methodName,
                    resultContract.TypeArguments[0],
                    createdAtType,
                    out var successBranch))
            {
                responseType = string.Empty;
                responseStatements = string.Empty;
                return false;
            }

            branches.Add(successBranch);
            foreach (var errorType in valueResult.TypeArguments.Skip(1))
            {
                if (!TryCreateErrorBranch(errorType, out var errorBranch))
                {
                    ReportInvalidEndpoint(
                        context,
                        apiType,
                        methodName,
                        $"error type '{errorType.ToDisplayString()}' does not derive from a supported HTTP error base type");
                    responseType = string.Empty;
                    responseStatements = string.Empty;
                    return false;
                }

                branches.Add(errorBranch);
                errorTypes.Add(errorType);
            }
        }
        else
        {
            if (!TryCreateSuccessBranch(
                    context,
                    apiType,
                    methodName,
                    applicationResultType,
                    createdAtType,
                    out var successBranch))
            {
                responseType = string.Empty;
                responseStatements = string.Empty;
                return false;
            }

            branches.Add(successBranch);
        }

        responseType = branches.Count == 1
            ? branches[0].ResultType
            : $"global::Microsoft.AspNetCore.Http.HttpResults.Results<{string.Join(", ", branches.Select(branch => branch.ResultType))}>";

        if (domainResult is null)
        {
            responseStatements = "        return " + branches[0].CreateExpression("result") + ";\n";
            return true;
        }

        var statements = new StringBuilder();
        if (isSuccessOr)
        {
            statements.AppendLine("        if (!result.TryGetFailure(out var failure))")
                .Append("            return ")
                .Append(branches[0].CreateExpression("result"))
                .AppendLine(";");
        }
        else
        {
            statements.AppendLine("        if (result.TryGetValue(out var value, out var failure))")
                .Append("            return ")
                .Append(branches[0].CreateExpression("value"))
                .AppendLine(";");
        }

        for (var index = 0; index < errorTypes.Count; index++)
        {
            var errorName = "error" + (index + 1);
            statements.Append("        if (failure.TryGet<")
                .Append(TypeName(errorTypes[index]))
                .Append(">(out var ")
                .Append(errorName)
                .AppendLine("))")
                .Append("            return ")
                .Append(branches[index + 1].CreateExpression(errorName))
                .AppendLine(";");
        }

        statements.AppendLine("        throw new global::System.InvalidOperationException(\"Generated endpoint received an invalid failure.\");");
        responseStatements = statements.ToString();
        return true;
    }

    private static bool TryCreateSuccessBranch(
        SourceProductionContext context,
        INamedTypeSymbol apiType,
        string methodName,
        ITypeSymbol valueType,
        INamedTypeSymbol? createdAtType,
        out HttpResultBranch branch)
    {
        if (IsType(valueType, SuccessMetadataName))
        {
            if (createdAtType is not null)
            {
                ReportInvalidEndpoint(
                    context,
                    apiType,
                    methodName,
                    "CreatedAt requires an application result value");
                branch = null!;
                return false;
            }

            branch = HttpResultBranch.NoContent();
            return true;
        }

        if (createdAtType is null)
        {
            branch = HttpResultBranch.Ok(valueType);
            return true;
        }

        if (!TryGetCreatedAtRouteProperty(createdAtType, valueType, out var routeProperty))
        {
            ReportInvalidEndpoint(
                context,
                apiType,
                methodName,
                $"CreatedAt request '{createdAtType.ToDisplayString()}' must have exactly one [FromRoute] property matching '{valueType.ToDisplayString()}'");
            branch = null!;
            return false;
        }

        var targetMethodName = GetEndpointMethodName(createdAtType);
        branch = HttpResultBranch.CreatedAtRoute(
            valueType,
            targetMethodName,
            routeProperty.BindingName);
        return true;
    }

    private static bool TryCreateErrorBranch(ITypeSymbol errorType, out HttpResultBranch branch)
    {
        if (InheritsFrom(errorType, "Shared.Domain.Abstractions.Errors.ForbiddenError"))
            branch = HttpResultBranch.Forbid(errorType);
        else if (InheritsFrom(errorType, "Shared.Domain.Abstractions.Errors.NotFoundError"))
            branch = HttpResultBranch.NotFound(errorType);
        else if (InheritsFrom(errorType, "Shared.Domain.Abstractions.Errors.ConflictError"))
            branch = HttpResultBranch.Conflict(errorType);
        else if (InheritsFrom(errorType, "Shared.Domain.Abstractions.Errors.ValidationError"))
            branch = HttpResultBranch.BadRequest(errorType);
        else if (InheritsFrom(errorType, "Shared.Domain.Abstractions.Errors.AuthenticationError"))
            branch = HttpResultBranch.Unauthorized(errorType);
        else
        {
            branch = null!;
            return false;
        }

        return true;
    }

    private static ImmutableArray<EffectiveProperty> GetEffectiveProperties(INamedTypeSymbol type)
    {
        var properties = new List<EffectiveProperty>();
        var names = new HashSet<string>(StringComparer.Ordinal);

        void Add(IPropertySymbol property)
        {
            if (!property.IsStatic && names.Add(property.Name))
                properties.Add(new EffectiveProperty(property.Name, property.Type, property));
        }

        foreach (var current in GetTypeHierarchy(type))
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>()
                         .Where(property => property.DeclaredAccessibility == Accessibility.Public))
                Add(property);

        foreach (var attribute in type.GetAttributes().Where(attribute =>
                     attribute.AttributeClass?.ToDisplayString() == IncludeAttributeMetadataName))
            AddSelectedSourceProperties(attribute, Add);

        foreach (var attribute in type.GetAttributes().Where(attribute =>
                     attribute.AttributeClass?.ToDisplayString() == OmitAttributeMetadataName))
        {
            if (attribute.ConstructorArguments.Length < 2 ||
                attribute.ConstructorArguments[0].Value is not INamedTypeSymbol sourceType)
                continue;

            var omitted = attribute.ConstructorArguments.Length >= 3
                ? new HashSet<string>(
                    attribute.ConstructorArguments[2].Values
                        .Select(value => value.Value as string)
                        .Where(value => value is not null)!,
                    StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);

            foreach (var include in sourceType.GetAttributes().Where(candidate =>
                         candidate.AttributeClass?.ToDisplayString() == IncludeAttributeMetadataName))
                AddSelectedSourceProperties(include, property =>
                {
                    if (!omitted.Contains(property.Name)) Add(property);
                });

            foreach (var current in GetTypeHierarchy(sourceType))
                foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
                    if (!omitted.Contains(property.Name)) Add(property);
        }

        return properties.ToImmutableArray();
    }

    private static void AddSelectedSourceProperties(
        AttributeData attribute,
        Action<IPropertySymbol> add)
    {
        if (attribute.ConstructorArguments.Length < 3 ||
            attribute.ConstructorArguments[0].Value is not INamedTypeSymbol sourceType)
            return;

        var sourceProperties = GetTypeHierarchy(sourceType)
            .SelectMany(current => current.GetMembers().OfType<IPropertySymbol>())
            .ToDictionary(property => property.Name, StringComparer.Ordinal);
        foreach (var name in attribute.ConstructorArguments[2].Values
                     .Select(value => value.Value as string)
                     .Where(value => value is not null))
            if (sourceProperties.TryGetValue(name!, out var property))
                add(property);
    }

    private static IEnumerable<INamedTypeSymbol> GetTypeHierarchy(INamedTypeSymbol type)
    {
        var hierarchy = new Stack<INamedTypeSymbol>();
        for (var current = type;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
            hierarchy.Push(current);

        while (hierarchy.Count > 0) yield return hierarchy.Pop();
    }

    private static AuthorizationMode GetAuthorizationMode(INamedTypeSymbol requestType)
    {
        var attribute = requestType.GetAttributes().FirstOrDefault(candidate =>
            candidate.AttributeClass?.ToDisplayString() == GenerateBindAttributeMetadataName);
        if (attribute is null || attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is null)
            return AuthorizationMode.None;

        return (AuthorizationMode)Convert.ToByte(attribute.ConstructorArguments[0].Value);
    }

    private static string GetEndpointMethodName(INamedTypeSymbol requestType)
    {
        const string suffix = "Request";
        var name = requestType.Name.EndsWith(suffix, StringComparison.Ordinal)
            ? requestType.Name.Substring(0, requestType.Name.Length - suffix.Length)
            : requestType.Name;
        return name + "Async";
    }

    private static bool InheritsFrom(ITypeSymbol type, string metadataName)
    {
        for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
            if (current.ToDisplayString() == metadataName)
                return true;
        return false;
    }

    private static bool IsNullableOf(ITypeSymbol type, string metadataName) =>
        type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            TypeArguments.Length: 1
        } nullable && IsType(nullable.TypeArguments[0], metadataName);

    private static bool IsType(ITypeSymbol type, string metadataName)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            type = type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        return type.ToDisplayString() == metadataName;
    }

    private static INamedTypeSymbol? GetResultContract(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType
            ? namedType.AllInterfaces.FirstOrDefault(candidate =>
                candidate.MetadataName == ResultContractMetadataName &&
                candidate.ContainingNamespace.ToDisplayString() == ResultNamespace)
            : null;

    private static bool IsSuccessOr(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType &&
        namedType.AllInterfaces.Any(candidate =>
            candidate.MetadataName == ResultMarkerMetadataName &&
            candidate.ContainingNamespace.ToDisplayString() == ResultNamespace) &&
        GetResultContract(namedType) is null;

    private static string TypeName(ITypeSymbol type) => type.ToDisplayString(FullyQualifiedTypeFormat);

    private static string LowerFirst(string value) =>
        value.Length == 0 ? value : char.ToLowerInvariant(value[0]) + value.Substring(1);

    private static string RemoveSuffix(string value, string suffix) =>
        value.EndsWith(suffix, StringComparison.Ordinal)
            ? value.Substring(0, value.Length - suffix.Length)
            : value;

    private static string AppendRouteParameter(string pattern, string parameter) =>
        string.IsNullOrEmpty(pattern)
            ? "{" + parameter + "}"
            : pattern.TrimEnd('/') + "/{" + parameter + "}";

    private static string EscapeXml(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static Location GetLocation(ISymbol symbol) =>
        symbol.Locations.FirstOrDefault(location => location.IsInSource) ?? Location.None;

    private static void ReportInvalidEndpoint(
        SourceProductionContext context,
        INamedTypeSymbol apiType,
        string endpoint,
        string reason) =>
        ReportInvalidEndpoint(context, GetLocation(apiType), endpoint, reason);

    private static void ReportInvalidEndpoint(
        SourceProductionContext context,
        Location location,
        string endpoint,
        string reason) =>
        context.ReportDiagnostic(Diagnostic.Create(
            InvalidEndpointSpecification,
            location,
            endpoint,
            reason));

    private sealed class EndpointTarget
    {
        public EndpointTarget(
            INamedTypeSymbol apiType,
            EndpointSpecification specification,
            string mapMethod,
            Location location,
            Location handlerLocation,
            Location? createdAtLocation,
            ImmutableArray<RouteParameterPattern> routeParameters,
            string endpointPattern,
            bool isRouteGroup)
        {
            ApiType = apiType;
            Specification = specification;
            MapMethod = mapMethod;
            Location = location;
            HandlerLocation = handlerLocation;
            CreatedAtLocation = createdAtLocation;
            RouteParameters = routeParameters;
            EndpointPattern = endpointPattern;
            IsRouteGroup = isRouteGroup;
        }

        public INamedTypeSymbol ApiType { get; }
        public EndpointSpecification Specification { get; }
        public string MapMethod { get; }
        public Location Location { get; }
        public Location HandlerLocation { get; }
        public Location? CreatedAtLocation { get; }
        public ImmutableArray<RouteParameterPattern> RouteParameters { get; }
        public string EndpointPattern { get; }
        public bool IsRouteGroup { get; }
    }

    private sealed class DocumentationFile
    {
        public DocumentationFile(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; }
        public string Content { get; }
    }

    private sealed class DocumentationEntry
    {
        public DocumentationEntry(string summary, Location location)
        {
            Summary = summary;
            Location = location;
        }

        public string Summary { get; }
        public Location Location { get; }
    }

    private sealed class EndpointSpecification
    {
        public EndpointSpecification(
            INamedTypeSymbol requestType,
            INamedTypeSymbol handlerType,
            INamedTypeSymbol? createdAtType)
        {
            RequestType = requestType;
            HandlerType = handlerType;
            CreatedAtType = createdAtType;
        }

        public INamedTypeSymbol RequestType { get; }
        public INamedTypeSymbol HandlerType { get; }
        public INamedTypeSymbol? CreatedAtType { get; }
    }

    private sealed class EffectiveProperty
    {
        public EffectiveProperty(string name, ITypeSymbol type, IPropertySymbol symbol)
        {
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public string Name { get; }
        public ITypeSymbol Type { get; }
        public IPropertySymbol Symbol { get; }
    }

    private sealed class HandlerContract
    {
        public HandlerContract(
            INamedTypeSymbol requestType,
            ITypeSymbol resultType,
            HandlerKind kind)
        {
            RequestType = requestType;
            ResultType = resultType;
            Kind = kind;
        }

        public INamedTypeSymbol RequestType { get; }
        public ITypeSymbol ResultType { get; }
        public HandlerKind Kind { get; }
    }

    private sealed class RouteProperty
    {
        public RouteProperty(string propertyName, string bindingName)
        {
            PropertyName = propertyName;
            BindingName = bindingName;
        }

        public string PropertyName { get; }
        public string BindingName { get; }
    }

    private sealed class RouteParameterPattern
    {
        public RouteParameterPattern(string name, bool isOptional)
        {
            Name = name;
            IsOptional = isOptional;
        }

        public string Name { get; }
        public bool IsOptional { get; }
    }

    private sealed class HttpResultBranch
    {
        private readonly Func<string, string> _createExpression;

        private HttpResultBranch(string resultType, bool isNoContent, Func<string, string> createExpression)
        {
            ResultType = resultType;
            IsNoContent = isNoContent;
            _createExpression = createExpression;
        }

        public string ResultType { get; }
        public bool IsNoContent { get; }
        public string CreateExpression(string value) => _createExpression(value);

        public static HttpResultBranch NoContent() => new(
            "global::Microsoft.AspNetCore.Http.HttpResults.NoContent",
            true,
            _ => "global::Microsoft.AspNetCore.Http.TypedResults.NoContent()");

        public static HttpResultBranch Ok(ITypeSymbol type) => new(
            $"global::Microsoft.AspNetCore.Http.HttpResults.Ok<{TypeName(type)}>",
            false,
            value => $"global::Microsoft.AspNetCore.Http.TypedResults.Ok({value})");

        public static HttpResultBranch CreatedAtRoute(
            ITypeSymbol type,
            string targetMethod,
            string routeProperty)
        {
            var routePropertyLiteral = SyntaxFactory.Literal(routeProperty).ToString();
            return new HttpResultBranch(
                $"global::Microsoft.AspNetCore.Http.HttpResults.CreatedAtRoute<{TypeName(type)}>",
                false,
                value =>
                    $"global::Microsoft.AspNetCore.Http.TypedResults.CreatedAtRoute({value}, nameof({targetMethod}), new global::Microsoft.AspNetCore.Routing.RouteValueDictionary {{ [{routePropertyLiteral}] = {value} }})");
        }

        public static HttpResultBranch NotFound(ITypeSymbol type) => new(
            $"global::Microsoft.AspNetCore.Http.HttpResults.NotFound<{TypeName(type)}>",
            false,
            value => $"global::Microsoft.AspNetCore.Http.TypedResults.NotFound({value})");

        public static HttpResultBranch Conflict(ITypeSymbol type) => new(
            $"global::Microsoft.AspNetCore.Http.HttpResults.Conflict<{TypeName(type)}>",
            false,
            value => $"global::Microsoft.AspNetCore.Http.TypedResults.Conflict({value})");

        public static HttpResultBranch BadRequest(ITypeSymbol type) => new(
            $"global::Microsoft.AspNetCore.Http.HttpResults.BadRequest<{TypeName(type)}>",
            false,
            value => $"global::Microsoft.AspNetCore.Http.TypedResults.BadRequest({value})");

        public static HttpResultBranch Forbid(ITypeSymbol type) => new(
            $"global::Shared.Presentation.Abstractions.Forbid<{TypeName(type)}>",
            false,
            value => $"new global::Shared.Presentation.Abstractions.Forbid<{TypeName(type)}>({value})");

        public static HttpResultBranch Unauthorized(ITypeSymbol type) => new(
            $"global::Shared.Presentation.Abstractions.Unauthorized<{TypeName(type)}>",
            false,
            value => $"new global::Shared.Presentation.Abstractions.Unauthorized<{TypeName(type)}>({value})");
    }

    private enum AuthorizationMode : byte
    {
        None = 0,
        Required = 1,
        Optional = 2
    }

    private enum HandlerKind : byte
    {
        Command,
        Query
    }
}
