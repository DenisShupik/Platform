using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Shared.Presentation.Transformers;

namespace Shared.Presentation.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("openapi",
            options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;

                options.AddSchemaTransformer<PrimitiveSchemaTransformer>();
                options.AddSchemaTransformer<RequiredSchemaTransformer>();
                options.AddSchemaTransformer<ValueObjectSchemaTransformer>();
                options.AddSchemaTransformer<SortSchemaTransformer>();
                options.AddSchemaTransformer<SetSchemaTransformer>();
                options.AddSchemaTransformer<ResultSchemaTransformer>();
                options.AddSchemaTransformer<DictionarySchemaTransformer>();
                options.AddSchemaTransformer<EnumSchemaTransformer>();
                options.AddOperationTransformer<EnhanceOperationTransformer>();
                options.AddOperationTransformer<GenerateBindOperationTransformer>();
                options.AddOperationTransformer<SecuritySchemeOperationTransformer>();
                options.AddOperationTransformer<CamelCaseParameterNameOperationTransformer>();
                options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
                options.AddDocumentTransformer<JsonPolymorphicDocumentTransformer>();
            });

        return services;
    }
}