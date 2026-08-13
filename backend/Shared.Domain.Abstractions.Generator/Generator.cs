using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Shared.Domain.Abstractions.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    private const int MaximumErrorCount = 7;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static sourceContext =>
        {
            AddSource(sourceContext, "Result.Runtime.g.cs", ResultSyntaxFactory.CreateRuntime());

            for (var errorCount = 1; errorCount <= MaximumErrorCount; errorCount++)
            {
                AddSource(
                    sourceContext,
                    $"Failure{errorCount}.g.cs",
                    ResultSyntaxFactory.CreateFailure(errorCount));
                AddSource(
                    sourceContext,
                    $"Result{errorCount + 1}.g.cs",
                    ResultSyntaxFactory.CreateResult(errorCount));
                AddSource(
                    sourceContext,
                    $"SuccessOr{errorCount}.g.cs",
                    ResultSyntaxFactory.CreateSuccessOr(errorCount));
            }
        });
    }

    private static void AddSource(
        IncrementalGeneratorPostInitializationContext context,
        string hintName,
        Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax source) =>
        context.AddSource(hintName, SourceText.From(source.ToFullString(), Encoding.UTF8));
}
