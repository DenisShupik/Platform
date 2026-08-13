using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Shared.Presentation.Generator;

public sealed partial class Generator
{
    private const string GenerateResultJsonConvertersAttributeMetadataName =
        "Shared.Presentation.Generator.Attributes.GenerateResultJsonConvertersAttribute";
    private const int MaximumResultErrorCount = 7;

    private void InitializeResultJsonConverters(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateResultJsonConvertersAttributeMetadataName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (_, _) => true)
            .Collect();

        context.RegisterSourceOutput(targets, static (sourceContext, requests) =>
        {
            if (requests.IsDefaultOrEmpty) return;

            AddResultJsonSource(
                sourceContext,
                "ResultJsonConverterFactory.g.cs",
                ResultJsonSyntaxFactory.CreateFactory(MaximumResultErrorCount));

            for (var errorCount = 1; errorCount <= MaximumResultErrorCount; errorCount++)
                AddResultJsonSource(
                    sourceContext,
                    $"ResultJsonConverter{errorCount + 1}.g.cs",
                    ResultJsonSyntaxFactory.CreateConverter(errorCount));
        });
    }

    private static void AddResultJsonSource(
        SourceProductionContext context,
        string hintName,
        CompilationUnitSyntax source) =>
        context.AddSource(hintName, SourceText.From(source.ToFullString(), Encoding.UTF8));
}
