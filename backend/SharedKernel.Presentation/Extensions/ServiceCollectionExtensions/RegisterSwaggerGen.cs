using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.Abstractions;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Presentation.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

namespace SharedKernel.Presentation.Extensions;

public static partial class ServiceCollectionExtensions
{
    private const string Suffix = "Type";

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
                    var defaultSchemaIdSelector = options.SchemaGeneratorOptions.SchemaIdSelector;
                    options.CustomSchemaIds(type =>
                    {
                        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SortCriteria<>))
                            return defaultSchemaIdSelector(type);
                        var name = type.GenericTypeArguments[0].Name;

                        if (name.EndsWith(Suffix, StringComparison.Ordinal))
                        {
                            name = name[..^Suffix.Length] + "Enum";
                        }

                        return name;
                    });

                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new Uri(keycloakOptions.Value.MetadataAddress)
                    });

                    options.UseOneOfForPolymorphism();
                    options.SelectDiscriminatorNameUsing(_ => "$type");
                    options.SupportNonNullableReferenceTypes();
                    options.UseAllOfToExtendReferenceSchemas();
                    options.DescribeAllParametersInCamelCase();
                    
                    setupAction?.Invoke(options);
                    options.OperationFilter<AddSecurityRequirementsOperationFilter>();
                    options.OperationFilter<AddOperationIdOperationFilter>();
                    options.SchemaFilter<AddPaginationLimitSchemaFilter>();
                    options.SchemaFilter<AddRequiredSchemaFilter>();
                    options.SchemaFilter<AddSetSchemaFilter>();

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