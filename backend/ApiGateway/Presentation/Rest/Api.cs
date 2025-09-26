using ApiGateway.Infrastructure.Interfaces;

namespace ApiGateway.Presentation.Rest;

public static class Api
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api");

        api.MapGet("openapi.json",
            async (IOpenApiAggregatorService openApiAggregator, CancellationToken cancellationToken) =>
            {
                var openApiJson = await openApiAggregator.GetOpenApiJson(cancellationToken);
                return TypedResults.Content(openApiJson, "application/json; charset=utf-8");
            });

        return app;
    }
}