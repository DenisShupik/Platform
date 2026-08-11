using System.Globalization;
using ApiGateway.Infrastructure.Interfaces;
using Shared.Domain.ValueObjects;

namespace ApiGateway.Presentation.Rest;

public static class Api
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api");

        api.MapGet("openapi.json",
            async (IOpenApiAggregatorService openApiAggregator, CancellationToken cancellationToken) =>
            {
                var locale = Locale.From(CultureInfo.CurrentUICulture.Name);
                var openApiJson = await openApiAggregator.GetOpenApiJson(locale, cancellationToken);
                return TypedResults.Content(openApiJson, "application/openapi+json; charset=utf-8");
            });

        return app;
    }
}
