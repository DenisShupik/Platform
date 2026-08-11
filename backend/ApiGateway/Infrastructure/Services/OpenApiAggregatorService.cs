using System.Security.Cryptography;
using System.Text;
using ApiGateway.Infrastructure.Interfaces;
using Microsoft.OpenApi;
using Yarp.ReverseProxy.Configuration;
using ZiggyCreatures.Caching.Fusion;

namespace ApiGateway.Infrastructure.Services;

public sealed class OpenApiAggregatorService : IOpenApiAggregatorService
{
    private static readonly string CacheKeyPrefix =
        $"openapi:json:{typeof(OpenApiAggregatorService).Assembly.ManifestModule.ModuleVersionId:N}";

    private readonly IProxyConfigProvider _proxyConfigProvider;
    private readonly IFusionCache _cache;

    public OpenApiAggregatorService(
        IProxyConfigProvider proxyConfigProvider,
        IFusionCacheProvider cacheProvider)
    {
        _proxyConfigProvider = proxyConfigProvider;
        _cache = cacheProvider.GetCache(Constants.CacheName);
    }

    private async Task<string> MergeOpenApiDocument(
        IReadOnlyCollection<OpenApiSource> sources,
        CancellationToken cancellationToken)
    {
        var readResults = await Task.WhenAll(sources.Select(async source =>
        {
            var result = await OpenApiDocument.LoadAsync(source.Url.ToString(), token: cancellationToken);
            return (source, result.Document);
        }));

        var merged = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = "API Gateway",
                Version = "1.0.0",
                Description = "Aggregated REST API for Platform services.",
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Identifier = "MIT"
                }
            },
            Servers = [new OpenApiServer { Url = "/" }],
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>(),
                Responses = new Dictionary<string, IOpenApiResponse>(),
                Parameters = new Dictionary<string, IOpenApiParameter>(),
                Examples = new Dictionary<string, IOpenApiExample>(),
                RequestBodies = new Dictionary<string, IOpenApiRequestBody>(),
                Headers = new Dictionary<string, IOpenApiHeader>(),
                SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>(),
                Links = new Dictionary<string, IOpenApiLink>(),
                Callbacks = new Dictionary<string, IOpenApiCallback>(),
                PathItems = new Dictionary<string, IOpenApiPathItem>()
            },
            Tags = new HashSet<OpenApiTag>()
        };

        foreach (var (source, document) in readResults)
        {
            if (document is null)
                throw new OpenApiException($"OpenAPI document from cluster '{source.ClusterId}' could not be read");

            foreach (var path in document.Paths)
                if (!merged.Paths.TryAdd(path.Key, path.Value))
                    throw new OpenApiException(
                        $"OpenAPI path '{path.Key}' is defined by more than one downstream service");

            if (document.Components == null) continue;

            var components = document.Components;
            MergeComponents(merged.Components.Schemas, components.Schemas, "schema", source.ClusterId);
            MergeComponents(merged.Components.Responses, components.Responses, "response", source.ClusterId);
            MergeComponents(merged.Components.Parameters, components.Parameters, "parameter", source.ClusterId);
            MergeComponents(merged.Components.Examples, components.Examples, "example", source.ClusterId);
            MergeComponents(merged.Components.RequestBodies, components.RequestBodies, "request body", source.ClusterId);
            MergeComponents(merged.Components.Headers, components.Headers, "header", source.ClusterId);
            MergeComponents(
                merged.Components.SecuritySchemes,
                components.SecuritySchemes,
                "security scheme",
                source.ClusterId);
            MergeComponents(merged.Components.Links, components.Links, "link", source.ClusterId);
            MergeComponents(merged.Components.Callbacks, components.Callbacks, "callback", source.ClusterId);
            MergeComponents(merged.Components.PathItems, components.PathItems, "path item", source.ClusterId);

            if (document.Tags is not null)
                foreach (var tag in document.Tags)
                    MergeTag(merged.Tags, tag, source.ClusterId);
        }

        new OpenApiWalker(new PropertyNamesVisitor()).Walk(merged);

        await using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        merged.SerializeAsV31(jsonWriter);

        return stringWriter.ToString();
    }

    private IReadOnlyList<OpenApiSource> GetSources()
    {
        var proxyConfig = _proxyConfigProvider.GetConfig();
        return proxyConfig.Clusters
            .OrderBy(cluster => cluster.ClusterId, StringComparer.Ordinal)
            .Select(cluster =>
            {
                var destination = cluster.Destinations?
                    .OrderBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => item.Value)
                    .FirstOrDefault()
                    ?? throw new OpenApiException(
                        $"Reverse proxy cluster '{cluster.ClusterId}' has no OpenAPI source destination");

                return new OpenApiSource(
                    cluster.ClusterId,
                    new Uri(new Uri(destination.Address), "api/openapi.json"));
            })
            .ToArray();
    }

    private static string CreateCacheKey(IEnumerable<OpenApiSource> sources)
    {
        var sourceIdentity = string.Join('|', sources.Select(source => $"{source.ClusterId}:{source.Url}"));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sourceIdentity)));
        return $"{CacheKeyPrefix}:{fingerprint}";
    }

    private static void MergeComponents<T>(
        IDictionary<string, T> target,
        IDictionary<string, T>? source,
        string componentType,
        string clusterId)
        where T : IOpenApiSerializable
    {
        if (source is null) return;

        foreach (var component in source)
        {
            if (target.TryAdd(component.Key, component.Value)) continue;
            if (Serialize(target[component.Key]) == Serialize(component.Value)) continue;

            throw new OpenApiException(
                $"OpenAPI {componentType} '{component.Key}' from cluster '{clusterId}' conflicts with another downstream service");
        }
    }

    private static void MergeTag(ISet<OpenApiTag> target, OpenApiTag tag, string clusterId)
    {
        var existing = target.FirstOrDefault(candidate => candidate.Name == tag.Name);
        if (existing is null)
        {
            target.Add(tag);
            return;
        }

        if (Serialize(existing) != Serialize(tag))
            throw new OpenApiException(
                $"OpenAPI tag '{tag.Name}' from cluster '{clusterId}' conflicts with another downstream service");
    }

    private static string Serialize(IOpenApiSerializable element)
    {
        using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        element.SerializeAsV31(jsonWriter);
        return stringWriter.ToString();
    }

    private sealed class PropertyNamesVisitor : OpenApiVisitorBase
    {
        public override void Visit(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchema openApiSchema &&
                openApiSchema.UnrecognizedKeywords?.TryGetValue("propertyNames", out var propertyNames) == true)
            {
                openApiSchema.UnrecognizedKeywords.Remove("propertyNames");
                openApiSchema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                openApiSchema.Extensions["propertyNames"] = new JsonNodeExtension(propertyNames);
            }

            base.Visit(schema);
        }
    }

    public ValueTask<string> GetOpenApiJson(CancellationToken cancellationToken)
    {
        var sources = GetSources();
        return _cache.GetOrSetAsync<string>(
            CreateCacheKey(sources),
            token => MergeOpenApiDocument(sources, token),
            token: cancellationToken);
    }

    private sealed record OpenApiSource(string ClusterId, Uri Url);
}
