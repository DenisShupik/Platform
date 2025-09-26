using ApiGateway.Infrastructure.Interfaces;
using Microsoft.OpenApi;
using Yarp.ReverseProxy.Configuration;
using ZiggyCreatures.Caching.Fusion;

namespace ApiGateway.Infrastructure.Services;

public sealed class OpenApiAggregatorService : IOpenApiAggregatorService
{
    private readonly IProxyConfigProvider _proxyConfigProvider;
    private readonly IFusionCache _cache;

    public OpenApiAggregatorService(IProxyConfigProvider proxyConfigProvider, IFusionCacheProvider cacheProvider)
    {
        _proxyConfigProvider = proxyConfigProvider;
        _cache = cacheProvider.GetCache(Constants.CacheName);
    }

    private async Task<string> MergeOpenApiDocument(CancellationToken cancellationToken)
    {
        var proxyConfig = _proxyConfigProvider.GetConfig();
        var openApiUrls = proxyConfig.Clusters
            .SelectMany(e => e.Destinations?.Values ?? [])
            .Select(e => new Uri(new Uri(e.Address), "api/openapi.json").ToString())
            .ToList();

        var readResults =
            await Task.WhenAll(openApiUrls.Select(e => OpenApiDocument.LoadAsync(e, token: cancellationToken)));

        var merged = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = "API Gateway",
                Version = "1.0.0"
            },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>(),
                SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>()
            }
        };

        foreach (var document in readResults.Select(e => e.Document))
        {
            if (document == null) continue;

            foreach (var path in document.Paths)
                merged.Paths[path.Key] = path.Value;

            if (document.Components == null) continue;

            var components = document.Components;
            
            if (components.Schemas != null)
                foreach (var schema in components.Schemas)
                    merged.Components.Schemas[schema.Key] = schema.Value;

            if (components.SecuritySchemes != null)
                foreach (var schema in components.SecuritySchemes)
                    merged.Components.SecuritySchemes[schema.Key] = schema.Value;
        }

        await using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        merged.SerializeAsV31(jsonWriter);

        return stringWriter.ToString();
    }

    public ValueTask<string> GetOpenApiJson(CancellationToken cancellationToken)
    {
        return _cache.GetOrSetAsync<string>("openapi:json", MergeOpenApiDocument, token: cancellationToken);
    }
}