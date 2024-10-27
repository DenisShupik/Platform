using Common.Filters;
using Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TopicService.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterSwaggerGen(
        this IServiceCollection services
    )
    {
        services
            .AddFluentValidationAutoValidation()
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

                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                    options.OperationFilter<AddInternalErrorResultOperationFilter>();
                    options.DescribeAllParametersInCamelCase();
                }
            );

        return services;
    }
}