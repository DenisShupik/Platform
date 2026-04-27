using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Presentation.Transformers;

namespace Shared.Presentation.Extensions;

public static class OpenApiOptionsExtensions
{
    public static void SetupOpenApi(this OpenApiOptions options)
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;

        options.AddSchemaTransformer<PrimitiveSchemaTransformer>();
        options.AddSchemaTransformer<RequiredSchemaTransformer>();
        options.AddSchemaTransformer<ValueObjectSchemaTransformer>();
        options.AddSchemaTransformer<SortSchemaTransformer>();
        options.AddSchemaTransformer<SetSchemaTransformer>();
        options.AddSchemaTransformer<ResultSchemaTransformer>();
        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddSchemaTransformer<DictionarySchemaTransformer>();
        options.AddOperationTransformer<EnhanceOperationTransformer>();
        options.AddOperationTransformer<GenerateBindOperationTransformer>();
        options.AddOperationTransformer<SecuritySchemeOperationTransformer>();
        options.AddOperationTransformer<CamelCaseParameterNameOperationTransformer>();
        options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
        options.AddDocumentTransformer<JsonPolymorphicDocumentTransformer>();
    }
}