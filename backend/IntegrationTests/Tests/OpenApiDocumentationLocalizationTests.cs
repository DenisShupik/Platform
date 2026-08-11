using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BinkyLabs.OpenApi.Overlays;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Net.Http.Headers;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Localization;
using Shared.Presentation.Middleware;

namespace IntegrationTests.Tests;

public sealed partial class OpenApiDocumentationLocalizationTests
{
    private static readonly string[] DocumentationFiles =
    [
        "CoreService/Documentation/Api.en.xml",
        "NotificationService/Documentation/Api.en.xml",
        "UserService/Documentation/Api.en.xml",
        "FileService/Documentation/Api.en.xml"
    ];

    private static readonly string[] ResponseStatusCodes =
    [
        "200", "201", "204", "400", "401", "403", "404", "406", "409", "413", "500"
    ];

    [GeneratedRegex(@"operationId == '([^']+)'")]
    private static partial Regex OperationIdRegex();

    [GeneratedRegex(@"name == '([^']+)'\)\]\.description$")]
    private static partial Regex TagNameRegex();

    [Test]
    public async Task RussianOverlay_CoversCanonicalOperationsAndAppliesStrictly(
        CancellationToken cancellationToken)
    {
        var backendDirectory = FindBackendDirectory();
        var canonicalOperations = DocumentationFiles
            .Select(path => XDocument.Load(Path.Combine(backendDirectory, path)))
            .SelectMany(document => document.Root!.Elements("operation"))
            .ToDictionary(
                operation => operation.Attribute("key")!.Value,
                operation => operation.Element("summary")!.Value,
                StringComparer.Ordinal);

        var overlayPath = Path.Combine(
            backendDirectory,
            "ApiGateway/Documentation/openapi.ru.overlay.json");
        var overlayJson = await File.ReadAllTextAsync(overlayPath, cancellationToken);
        using var overlaySource = JsonDocument.Parse(overlayJson);
        var operationTranslations = overlaySource.RootElement.GetProperty("actions")
            .EnumerateArray()
            .Select(action => new
            {
                Match = OperationIdRegex().Match(action.GetProperty("target").GetString()!),
                Translation = action.GetProperty("update").GetString()
            })
            .Where(item => item.Match.Success)
            .ToDictionary(
                item => item.Match.Groups[1].Value,
                item => item.Translation!,
                StringComparer.Ordinal);

        await Assert.That(operationTranslations.Keys)
            .IsEquivalentTo(canonicalOperations.Keys);
        foreach (var summary in canonicalOperations.Values)
            await Assert.That(ContainsCyrillic(summary)).IsFalse();
        foreach (var summary in operationTranslations.Values)
            await Assert.That(ContainsCyrillic(summary)).IsTrue();

        var (overlay, diagnostic) = await OverlayDocument.ParseAsync(
            overlayJson,
            "json",
            new OverlayReaderSettings(),
            cancellationToken);
        var overlayDiagnostic = diagnostic
                                ?? throw new InvalidOperationException("Overlay parser returned no diagnostics");
        var parsedOverlay = overlay
                            ?? throw new InvalidOperationException("Overlay parser returned no document");
        await Assert.That(overlayDiagnostic.Errors).IsEmpty();

        var canonicalDocument = CreateCanonicalDocument(
            canonicalOperations,
            overlaySource.RootElement.GetProperty("actions"));
        var canonicalStructure = canonicalDocument.DeepClone();
        await using var canonicalStream = new MemoryStream(
            Encoding.UTF8.GetBytes(canonicalDocument.ToJsonString()),
            writable: false);

        var result = await parsedOverlay.ApplyToDocumentStreamAsync(
            canonicalStream,
            "json",
            new OverlayReaderSettings(),
            strict: true,
            cancellationToken);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Diagnostic.Errors).IsEmpty();
        await Assert.That(result.Document).IsNotNull();

        foreach (var path in result.Document!["paths"]!.AsObject())
        {
            var operation = path.Value!["get"]!;
            var operationId = operation["operationId"]!.GetValue<string>();
            await Assert.That(operation["summary"]!.GetValue<string>())
                .IsEqualTo(operationTranslations[operationId]);
        }

        RemoveDocumentationText(canonicalStructure);
        var localizedStructure = result.Document.DeepClone();
        RemoveDocumentationText(localizedStructure);
        await Assert.That(JsonNode.DeepEquals(canonicalStructure, localizedStructure)).IsTrue();
    }

    [Test]
    public async Task PublicOpenApi_RequiresLocaleAndAppliesRepresentationHeaders()
    {
        var missingLocaleContext = new DefaultHttpContext();
        missingLocaleContext.Request.Path = "/api/openapi.json";
        missingLocaleContext.Response.Body = new MemoryStream();
        var strictMiddleware = new RequireApiLocaleMiddleware(
            _ => Task.CompletedTask,
            requireOpenApiLocale: true);

        await strictMiddleware.InvokeAsync(missingLocaleContext);

        await Assert.That(missingLocaleContext.Response.StatusCode)
            .IsEqualTo(StatusCodes.Status406NotAcceptable);
        await Assert.That(missingLocaleContext.Response.Headers[HeaderNames.Vary].ToString())
            .IsEqualTo(HeaderNames.AcceptLanguage);

        var russianContext = new DefaultHttpContext();
        russianContext.Request.Path = "/api/openapi.json";
        russianContext.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(
            new RequestCulture(Locale.RussianCode),
            new StrictAcceptLanguageRequestCultureProvider()));
        var localizedMiddleware = new RequireApiLocaleMiddleware(
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            },
            requireOpenApiLocale: true);

        await localizedMiddleware.InvokeAsync(russianContext);

        await Assert.That(russianContext.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
        await Assert.That(russianContext.Response.Headers[HeaderNames.ContentLanguage].ToString())
            .IsEqualTo(Locale.RussianCode);
        await Assert.That(russianContext.Response.Headers[HeaderNames.Vary].ToString())
            .IsEqualTo(HeaderNames.AcceptLanguage);
    }

    private static JsonObject CreateCanonicalDocument(
        IReadOnlyDictionary<string, string> operations,
        JsonElement actions)
    {
        var responses = new JsonObject();
        foreach (var statusCode in ResponseStatusCodes)
            responses[statusCode] = new JsonObject
            {
                ["description"] = "Canonical response",
                ["headers"] = new JsonObject
                {
                    ["Content-Language"] = new JsonObject
                    {
                        ["description"] = "Canonical response locale"
                    }
                }
            };

        var paths = new JsonObject();
        var index = 0;
        foreach (var (operationId, summary) in operations)
            paths[$"/test/{index++}"] = new JsonObject
            {
                ["get"] = new JsonObject
                {
                    ["operationId"] = operationId,
                    ["summary"] = summary,
                    ["parameters"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["name"] = "Accept-Language",
                            ["in"] = "header",
                            ["description"] = "Canonical requested locale"
                        }
                    },
                    ["responses"] = responses.DeepClone()
                }
            };

        var tagNames = actions.EnumerateArray()
            .Select(action => TagNameRegex().Match(action.GetProperty("target").GetString()!))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        return new JsonObject
        {
            ["openapi"] = "3.2.0",
            ["info"] = new JsonObject
            {
                ["title"] = "API Gateway",
                ["version"] = "1.0.0",
                ["description"] = "Canonical API description"
            },
            ["paths"] = paths,
            ["tags"] = new JsonArray(tagNames.Select(name => (JsonNode)new JsonObject
            {
                ["name"] = name,
                ["description"] = "Canonical tag description"
            }).ToArray()),
            ["components"] = new JsonObject
            {
                ["schemas"] = new JsonObject
                {
                    ["InternalNotificationsPagedDto"] = new JsonObject
                    {
                        ["properties"] = new JsonObject
                        {
                            ["totalCount"] = new JsonObject
                            {
                                ["description"] = "Canonical total count"
                            }
                        }
                    },
                    ["GetThreadSubscriptionStatusQueryResult"] = new JsonObject
                    {
                        ["properties"] = new JsonObject
                        {
                            ["isSubscribed"] = new JsonObject
                            {
                                ["description"] = "Canonical subscription status"
                            }
                        }
                    }
                }
            }
        };
    }

    private static void RemoveDocumentationText(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var propertyName in jsonObject.Select(property => property.Key).ToArray())
            {
                if (propertyName is "title" or "summary" or "description")
                    jsonObject.Remove(propertyName);
                else
                    RemoveDocumentationText(jsonObject[propertyName]);
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray) RemoveDocumentationText(item);
        }
    }

    private static bool ContainsCyrillic(string value) =>
        value.Any(character => character is >= '\u0400' and <= '\u04ff');

    private static string FindBackendDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Backend.slnx")))
                return directory.FullName;

        throw new DirectoryNotFoundException("Could not locate the backend directory");
    }
}
