using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharedKernel.Presentation.Filters;
using SharedKernel.Presentation.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

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

                    setupAction?.Invoke(options);
                    options.OperationFilter<AddSecurityRequirementsOperationFilter>();
                    options.OperationFilter<AddOperationIdOperationFilter>();
                    options.SchemaFilter<AddRequiredSchemaFilter>();

                    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                        .ToList();

                    var xmlFilePaths = xmlFiles
                        .Select(fileName => Path.Combine(AppContext.BaseDirectory, fileName))
                        .Where(File.Exists)
                        .ToArray();

                    foreach (var xmlFilePath in xmlFilePaths)
                    {
                        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
                    }

                    options.AddEnumsWithValuesFixFilters(fixEnumsOptions =>
                    {
                        fixEnumsOptions.ApplySchemaFilter = true;
                        fixEnumsOptions.XEnumNamesAlias = "x-enum-varnames";
                        fixEnumsOptions.XEnumDescriptionsAlias = "x-enum-descriptions";
                        fixEnumsOptions.ApplyParameterFilter = true;
                        fixEnumsOptions.ApplyDocumentFilter = true;
                        fixEnumsOptions.IncludeDescriptions = true;
                        fixEnumsOptions.IncludeXEnumRemarks = true;
                        fixEnumsOptions.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;
                        fixEnumsOptions.NewLine = "\n";
                        foreach (var xmlFilePath in xmlFilePaths)
                        {
                            fixEnumsOptions.IncludeXmlCommentsFrom(xmlFilePath);
                        }
                    });
                }
            );

        return services;
    }
}