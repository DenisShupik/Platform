using Microsoft.AspNetCore.OpenApi;
using Shared.Presentation.Transformers;

namespace Shared.Presentation.Extensions;

public static class OpenApiOptionsExtensions
{
    public static void SetupOpenApi(this OpenApiOptions options)
    {
        options.CreateSchemaReferenceId = OpenApiSchemaReferenceId.Create;

        options.AddSchemaTransformer<ErrorSchemaTransformer>();
        options.AddSchemaTransformer<RequiredSchemaTransformer>();
        options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>();
        options.AddSchemaTransformer<ValueObjectSchemaTransformer>();
        options.AddSchemaTransformer<CollectionSchemaTransformer>();
        options.AddSchemaTransformer<SortSchemaTransformer>();
        options.AddSchemaTransformer<SetSchemaTransformer>();
        options.AddSchemaTransformer<ResultSchemaTransformer>();
        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddSchemaTransformer<DictionarySchemaTransformer>();
        options.AddSchemaTransformer<SchemaReferenceIdTransformer>();
        options.AddOperationTransformer<OperationIdOperationTransformer>();
        options.AddOperationTransformer<GenerateBindOperationTransformer>();
        options.AddOperationTransformer<SecuritySchemeOperationTransformer>();
        options.AddOperationTransformer<ApiContractOperationTransformer>();
        options.AddOperationTransformer<DiscriminatedErrorResponseOperationTransformer>();
        options.AddDocumentTransformer<ApiTagDescriptionDocumentTransformer>();
        options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
        options.AddDocumentTransformer<SchemaReferenceDocumentTransformer>();
        options.AddDocumentTransformer<JsonPolymorphicDocumentTransformer>();
    }
}
