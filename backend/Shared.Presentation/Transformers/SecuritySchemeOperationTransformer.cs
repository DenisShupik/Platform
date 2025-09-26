using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shared.Presentation.Transformers;

public sealed class SecuritySchemeOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (!metadata.OfType<AuthorizeAttribute>().Any())
            return Task.CompletedTask;

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        var hasAllowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

        operation.Security = new List<OpenApiSecurityRequirement>();
        if (hasAllowAnonymous) operation.Security.Add(new OpenApiSecurityRequirement());

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Constants.SecuritySchemeName, context.Document)] = []
        });
        return Task.CompletedTask;
    }
}