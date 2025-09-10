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
                options.AddSchemaTransformer<SortSchemaTransformer>();
                options.AddSchemaTransformer<EnumTransformer>();
                options.AddOperationTransformer<GenerateBindOperationTransformer>();
                options.AddOperationTransformer<OperationIdTransformer>();
                options.AddOperationTransformer<SecuritySchemeOperationTransformer>();
                options.AddOperationTransformer((operation, context, ct) =>
                {
                    var openApiParameters = operation.Parameters;
                    if (openApiParameters == null) return Task.CompletedTask;
                    foreach (var parameter in openApiParameters)
                    {
                        if (parameter is not OpenApiParameter schema || schema.Name == null) continue;
                        schema.Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(schema.Name);
                    }

                    return Task.CompletedTask;
                });
                options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();
                options.AddDocumentTransformer<JsonPolymorphicDocumentTransformer>();
            });

        return services;
    }
}