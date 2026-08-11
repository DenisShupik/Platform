using System.Security.Cryptography;
using System.Text;
using ApiGateway.Infrastructure.Interfaces;
using BinkyLabs.OpenApi.Overlays;
using Microsoft.OpenApi;
using Shared.Domain.ValueObjects;
using Yarp.ReverseProxy.Configuration;
using ZiggyCreatures.Caching.Fusion;

namespace ApiGateway.Infrastructure.Services;

public sealed class OpenApiAggregatorService : IOpenApiAggregatorService
{
    private const string RussianOverlayResourceName =
        "ApiGateway.Documentation.openapi.ru.overlay.json";

    private static readonly string CacheKeyPrefix =
        $"openapi:json:{typeof(OpenApiAggregatorService).Assembly.ManifestModule.ModuleVersionId:N}";

    private readonly IProxyConfigProvider _proxyConfigProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFusionCache _cache;
    private readonly ILogger<OpenApiAggregatorService> _logger;
    private readonly Lazy<Task<OverlayDocument>> _russianOverlay;

    public OpenApiAggregatorService(
        IProxyConfigProvider proxyConfigProvider,
        IHttpClientFactory httpClientFactory,
        IFusionCacheProvider cacheProvider,
        ILogger<OpenApiAggregatorService> logger)
    {
        _proxyConfigProvider = proxyConfigProvider;
        _httpClientFactory = httpClientFactory;
        _cache = cacheProvider.GetCache(Constants.CacheName);
        _logger = logger;
        _russianOverlay = new Lazy<Task<OverlayDocument>>(LoadRussianOverlayAsync);
    }

    private async Task<string> MergeOpenApiDocument(
        IReadOnlyCollection<OpenApiSource> sources,
        CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(OpenApiAggregatorService));
        var readResults = await Task.WhenAll(sources.Select(async source =>
        {
            var readerSettings = new Microsoft.OpenApi.Reader.OpenApiReaderSettings
            {
                HttpClient = httpClient
            };
            var result = await OpenApiDocument.LoadAsync(
                source.Url.ToString(),
                readerSettings,
                cancellationToken);
            var diagnostic = result.Diagnostic
                             ?? throw new OpenApiException(
                                 $"OpenAPI document from cluster '{source.ClusterId}' has no parse diagnostics");
            if (diagnostic.Errors.Count > 0)
                throw new OpenApiException(
                    $"OpenAPI document from cluster '{source.ClusterId}' contains parse errors: " +
                    FormatDiagnostics(diagnostic.Errors));

            if (diagnostic.SpecificationVersion != OpenApiSpecVersion.OpenApi3_2)
                throw new OpenApiException(
                    $"OpenAPI document from cluster '{source.ClusterId}' must use OpenAPI 3.2");

            if (diagnostic.Warnings.Count > 0)
                _logger.LogWarning(
                    "OpenAPI document from cluster {ClusterId} contains parse warnings: {Warnings}",
                    source.ClusterId,
                    FormatDiagnostics(diagnostic.Warnings));

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
                PathItems = new Dictionary<string, IOpenApiPathItem>(),
                MediaTypes = new Dictionary<string, IOpenApiMediaType>()
            },
            Tags = new HashSet<OpenApiTag>(),
            Webhooks = new Dictionary<string, IOpenApiPathItem>(),
            Security = []
        };

        foreach (var (source, document) in readResults)
        {
            if (document is null)
                throw new OpenApiException($"OpenAPI document from cluster '{source.ClusterId}' could not be read");

            foreach (var path in document.Paths)
                if (!merged.Paths.TryAdd(path.Key, path.Value))
                    throw new OpenApiException(
                        $"OpenAPI path '{path.Key}' is defined by more than one downstream service");

            MergeComponents(merged.Webhooks, document.Webhooks, "webhook", source.ClusterId);

            if (document.Tags is not null)
                foreach (var tag in document.Tags)
                    MergeTag(merged.Tags, tag, source.ClusterId);

            if (document.Security is not null)
                foreach (var requirement in document.Security)
                    MergeSecurityRequirement(merged.Security, requirement);

            MergeJsonSchemaDialect(merged, document, source.ClusterId);

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
            MergeComponents(merged.Components.MediaTypes, components.MediaTypes, "media type", source.ClusterId);

        }

        var validationErrors = merged.Validate(ValidationRuleSet.GetDefaultRuleSet()).ToArray();
        if (validationErrors.Length > 0)
            throw new OpenApiException(
                "Aggregated OpenAPI document is invalid: " + FormatDiagnostics(validationErrors));

        await using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        merged.SerializeAsV32(jsonWriter);

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

    private static string CreateCacheKey(IEnumerable<OpenApiSource> sources, string locale)
    {
        var sourceIdentity = string.Join('|', sources.Select(source => $"{source.ClusterId}:{source.Url}"));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sourceIdentity)));
        return $"{CacheKeyPrefix}:{fingerprint}:{locale}";
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

    private static void MergeSecurityRequirement(
        IList<OpenApiSecurityRequirement> target,
        OpenApiSecurityRequirement requirement)
    {
        var serialized = Serialize(requirement);
        if (!target.Any(candidate => Serialize(candidate) == serialized))
            target.Add(requirement);
    }

    private static void MergeJsonSchemaDialect(
        OpenApiDocument target,
        OpenApiDocument source,
        string clusterId)
    {
        if (source.JsonSchemaDialect is null) return;
        if (target.JsonSchemaDialect is null)
        {
            target.JsonSchemaDialect = source.JsonSchemaDialect;
            return;
        }

        if (target.JsonSchemaDialect != source.JsonSchemaDialect)
            throw new OpenApiException(
                $"OpenAPI JSON Schema dialect from cluster '{clusterId}' conflicts with another downstream service");
    }

    private static string FormatDiagnostics<T>(IEnumerable<T> diagnostics) =>
        string.Join("; ", diagnostics.Select(static diagnostic => diagnostic?.ToString()));

    private static string Serialize(IOpenApiSerializable element)
    {
        using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        element.SerializeAsV32(jsonWriter);
        return stringWriter.ToString();
    }

    private static async Task<OverlayDocument> LoadRussianOverlayAsync()
    {
        await using var stream = typeof(OpenApiAggregatorService).Assembly
            .GetManifestResourceStream(RussianOverlayResourceName)
            ?? throw new OpenApiException(
                $"Embedded OpenAPI overlay '{RussianOverlayResourceName}' was not found");

        var result = await OverlayDocument.LoadFromStreamAsync(
            stream,
            "json",
            new OverlayReaderSettings(),
            CancellationToken.None);

        var diagnostic = result.Diagnostic
                         ?? throw new OpenApiException("Russian OpenAPI overlay has no parse diagnostics");
        if (diagnostic.Errors.Count > 0)
            throw new OpenApiException(
                "Russian OpenAPI overlay is invalid: " +
                FormatDiagnostics(diagnostic.Errors));

        return result.Document
               ?? throw new OpenApiException("Russian OpenAPI overlay could not be parsed");
    }

    private async Task<string> ApplyRussianOverlayAsync(
        string canonicalDocument,
        CancellationToken cancellationToken)
    {
        var overlay = await _russianOverlay.Value;
        await using var source = new MemoryStream(
            Encoding.UTF8.GetBytes(canonicalDocument),
            writable: false);

        var result = await overlay.ApplyToDocumentStreamAndLoadAsync(
            source,
            new Uri("https://platform.invalid/api/openapi.json"),
            "json",
            new OverlayReaderSettings(),
            strict: true,
            cancellationToken);

        if (!result.IsSuccessful || result.Document is null)
        {
            var overlayErrors = result.Diagnostic?.Errors ?? [];
            var openApiErrors = result.OpenApiDiagnostic?.Errors ?? [];
            throw new OpenApiException(
                "Russian OpenAPI overlay could not be applied: " +
                FormatDiagnostics(overlayErrors.Concat(openApiErrors)));
        }

        var validationErrors = result.Document.Validate(ValidationRuleSet.GetDefaultRuleSet()).ToArray();
        if (validationErrors.Length > 0)
            throw new OpenApiException(
                "Localized OpenAPI document is invalid: " + FormatDiagnostics(validationErrors));

        return Serialize(result.Document);
    }

    public async ValueTask<string> GetOpenApiJson(
        Locale locale,
        CancellationToken cancellationToken)
    {
        var sources = GetSources();
        var canonicalDocument = await _cache.GetOrSetAsync<string>(
            CreateCacheKey(sources, Locale.EnglishCode),
            token => MergeOpenApiDocument(sources, token),
            token: cancellationToken);

        if (locale == Locale.English) return canonicalDocument;

        return await _cache.GetOrSetAsync<string>(
            CreateCacheKey(sources, Locale.RussianCode),
            token => ApplyRussianOverlayAsync(canonicalDocument, token),
            token: cancellationToken);
    }

    private sealed record OpenApiSource(string ClusterId, Uri Url);
}
