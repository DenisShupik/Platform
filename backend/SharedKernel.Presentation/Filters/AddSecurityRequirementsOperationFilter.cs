using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Presentation.Filters;

public sealed class AddSecurityRequirementsOperationFilter : IOperationFilter
{
    private static readonly OpenApiSecurityRequirement OAuth2Requirement = new()
    {
        [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            }
        ] = []
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (!metadata.OfType<AuthorizeAttribute>().Any())
            return;

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        operation.Security = new List<OpenApiSecurityRequirement>();

        var hasAllowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

        if (hasAllowAnonymous) operation.Security.Add(new OpenApiSecurityRequirement());
        
        operation.Security.Add(OAuth2Requirement);
    }
}