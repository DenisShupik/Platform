using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharedKernel.Presentation.Filters;
using SharedKernel.Presentation.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterSwaggerGen(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? setupAction = null
    )
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

        services.AddOptions<SwaggerGenOptions>()
            .Configure<IOptions<KeycloakOptions>>((options, keycloakOptions) =>
                {
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new Uri(keycloakOptions.Value.MetadataAddress)
                    });

                    options.UseOneOfForPolymorphism();
                    options.SelectDiscriminatorNameUsing(_ => "type");
                    options.SupportNonNullableReferenceTypes();
                    options.UseAllOfToExtendReferenceSchemas();
                    options.DescribeAllParametersInCamelCase();
                    options.AddEnumsWithValuesFixFilters();
                    setupAction?.Invoke(options);
                    options.OperationFilter<AddSecurityRequirementsOperationFilter>();
                    options.OperationFilter<AddOperationIdOperationFilter>();
                    options.SchemaFilter<AddRequiredSchemaFilter>();

                   

                    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                        .ToList();
                    foreach (var xmlFilePath in xmlFiles
                                 .Select(fileName => Path.Combine(AppContext.BaseDirectory, fileName))
                                 .Where(File.Exists))
                    {
                        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
                    }
                }
            );

        return services;
    }
}