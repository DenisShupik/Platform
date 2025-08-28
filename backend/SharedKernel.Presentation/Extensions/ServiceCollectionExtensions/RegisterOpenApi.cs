using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using SharedKernel.Presentation.Transformers;

namespace SharedKernel.Presentation.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("openapi",
            options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                options.AddSchemaTransformer<RequiredTransformer>();
                options.AddSchemaTransformer<ValueObjectTransformer>();
                options.AddSchemaTransformer<SetTransformer>();
                options.AddSchemaTransformer<SortCriteriaTransformer>();
                options.AddSchemaTransformer<EnumTransformer>();
                options.AddOperationTransformer<OperationIdTransformer>();
                options.AddOperationTransformer<SecuritySchemeOperationTransformer>();
                options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
            });

        return services;
    }
}