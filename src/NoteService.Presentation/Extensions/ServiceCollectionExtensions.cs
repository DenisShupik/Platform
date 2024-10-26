using Common.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NoteService.Presentation.Extensions;

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
                    options.AddSecurityDefinition("OIDC", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new Uri(keycloakOptions.Value.MetadataAddress),
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow()
                        }
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OIDC" }
                            },
                            []
                        }
                    });

                    options.OperationFilter<AuthorizeCheckOperationFilter>();
                    options.DescribeAllParametersInCamelCase();
                }
            );

        return services;
    }
}

public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize =
            context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ??
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (!hasAuthorize || hasAllowAnonymous) return;
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OIDC" }
                    },
                    []
                }
            }
        };
    }
}