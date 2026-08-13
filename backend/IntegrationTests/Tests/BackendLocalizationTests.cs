using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Localization;
using Shared.Tests.Services;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace IntegrationTests.Tests;

public sealed class BackendLocalizationTests
{
    [ClassDataSource<CoreServiceTestsFixture<BackendLocalizationTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<BackendLocalizationTests> Fixture { get; init; }

    [Test]
    public async Task ValidationProblem_UsesRequestedLocaleAndStableCode(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));

        using var response = await client.GetAsync("api/forums?limit=invalid", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(response.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/problem+json");
        await Assert.That(response.Content.Headers.ContentLanguage).Contains(Locale.RussianCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        await Assert.That(json.RootElement.GetProperty("code").GetString())
            .IsEqualTo("validation_failed");
        await Assert.That(json.RootElement.GetProperty("traceId").GetString()).IsNotEmpty();
        var error = json.RootElement.GetProperty("errors").GetProperty("limit");
        await Assert.That(error.GetProperty("code").GetString())
            .IsEqualTo("cannot_parse_input_value");
        await Assert.That(error.GetProperty("message").GetString()).Contains("формат");
    }

    [Test]
    public async Task GeneratedBinder_AggregatesRouteAndJsonBodyErrors(
        CancellationToken cancellationToken)
    {
        var handler = new UserTokenService.Handler(
            Fixture.InfrastructureFixture.UserTokenService,
            () => Fixture.TestUsername);
        using var client = Fixture.CreateDefaultClient(handler);
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.EnglishCode));
        using var content = new StringContent("{", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(
            "api/threads/not-a-guid/posts",
            content,
            cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var errors = json.RootElement.GetProperty("errors");
        await Assert.That(errors.GetProperty("threadId").GetProperty("code").GetString())
            .IsEqualTo("cannot_parse_input_value");
        var bodyError = errors.EnumerateObject()
            .Single(error => error.Name.StartsWith("body", StringComparison.Ordinal));
        await Assert.That(bodyError.Value.GetProperty("code").GetString())
            .IsEqualTo("invalid_json_body");
    }

    [Test]
    public async Task TitleFiltering_IsCaseInsensitiveAndIndependentOfRequestCulture(
        CancellationToken cancellationToken)
    {
        var moderator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var forumId = await moderator.CreateForumAsync(new()
        {
            Title = ForumTitle.From("Culture Search Forum")
        }, cancellationToken);

        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));
        using var response = await client.GetAsync(
            "api/forums?title=CULTURE%20SEARCH",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await Assert.That(responseBody).Contains(forumId.Value.ToString());
    }

    [Test]
    public async Task ResponseLocalization_NormalizesHeadersCopiedFromDownstream()
    {
        var context = new DefaultHttpContext();
        context.Response.Headers[HeaderNames.ContentLanguage] =
            new StringValues([Locale.RussianCode, Locale.RussianCode]);
        context.Response.Headers[HeaderNames.Vary] =
            new StringValues(["Origin", HeaderNames.AcceptLanguage, HeaderNames.AcceptLanguage]);

        ApiResponseLocalization.Apply(context.Response, Locale.RussianCode);

        var contentLanguages = context.Response.Headers[HeaderNames.ContentLanguage]
            .Select(value => value!)
            .ToArray();
        await Assert.That(contentLanguages)
            .IsEquivalentTo([Locale.RussianCode]);
        await Assert.That(context.Response.Headers[HeaderNames.Vary].ToString())
            .IsEqualTo($"Origin, {HeaderNames.AcceptLanguage}");
    }

    [Test]
    public async Task OpenApi_KeepsDomainDiscriminatorInsideProblemDetailsUnion(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);
        response.EnsureSuccessStatusCode();

        await Assert.That(response.Content.Headers.ContentLanguage)
            .IsEquivalentTo([Locale.EnglishCode]);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        await Assert.That(json.RootElement.GetProperty("openapi").GetString()).IsEqualTo("3.2.0");
        var paths = json.RootElement.GetProperty("paths");

        foreach (var path in paths.EnumerateObject())
            foreach (var method in path.Value.EnumerateObject().Where(property =>
                         property.Name is "get" or "post" or "put" or "patch" or "delete"))
            {
                var summary = method.Value.GetProperty("summary").GetString();
                await Assert.That(summary).IsNotNull();
                await Assert.That(summary!).IsNotEmpty();
                await Assert.That(summary!.Any(character => character is >= '\u0400' and <= '\u04ff'))
                    .IsFalse();
            }

        var bulkPath = paths.GetProperty("/api/posts/bookmarks/bulk/{postIds}");
        await Assert.That(bulkPath.TryGetProperty("get", out _)).IsTrue();

        var singularPath = paths.GetProperty("/api/posts/bookmarks/{postId}");
        await Assert.That(singularPath.TryGetProperty("post", out _)).IsTrue();
        await Assert.That(singularPath.TryGetProperty("delete", out _)).IsTrue();
        await Assert.That(paths.TryGetProperty("/api/posts/bookmarks/{postIds}", out _)).IsFalse();

        var content = paths
            .GetProperty("/api/search")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("400")
            .GetProperty("content");
        var domainSchema = content
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(domainSchema.GetProperty("discriminator")
                .GetProperty("propertyName").GetString())
            .IsEqualTo("$type");

        var problemSchema = content
            .GetProperty("application/problem+json")
            .GetProperty("schema");
        var branches = problemSchema.GetProperty("anyOf").EnumerateArray().ToArray();
        await Assert.That(branches.Any(branch =>
                branch.TryGetProperty("$ref", out var reference) &&
                reference.GetString()!.EndsWith("/ApiProblemDetails", StringComparison.Ordinal)))
            .IsTrue();
        await Assert.That(branches.Any(branch =>
                branch.TryGetProperty("$ref", out var reference) &&
                reference.GetString()!.EndsWith("/ApiValidationProblemDetails", StringComparison.Ordinal)))
            .IsTrue();

        var problemDetails = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ApiProblemDetails");
        await Assert.That(problemDetails.GetProperty("properties").TryGetProperty("extensions", out _))
            .IsFalse();
        await Assert.That(problemDetails.GetProperty("required")
                .EnumerateArray()
                .Any(value => value.GetString() == "extensions"))
            .IsFalse();

        var rowVersionSchema = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(nameof(RowVersion));

        await Assert.That(rowVersionSchema.GetProperty("type").GetString()).IsEqualTo("integer");
        await Assert.That(rowVersionSchema.GetProperty("format").GetString()).IsEqualTo("uint32");
        await Assert.That(rowVersionSchema.GetProperty("minimum").GetUInt32()).IsEqualTo(uint.MinValue);
        await Assert.That(rowVersionSchema.GetProperty("maximum").GetUInt32()).IsEqualTo(uint.MaxValue);

        var forumNotFoundError = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ForumNotFoundError");
        var errorDiscriminator = forumNotFoundError
            .GetProperty("properties")
            .GetProperty("$type");
        await Assert.That(errorDiscriminator.GetProperty("const").GetString())
            .IsEqualTo("ForumNotFoundError");

        var bulkResponseSchema = paths
            .GetProperty("/api/forums/bulk/{forumIds}")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(bulkResponseSchema.GetProperty("propertyNames").GetProperty("$ref").GetString())
            .EndsWith("/ForumId");

        var resultSchema = bulkResponseSchema.GetProperty("additionalProperties");
        var resultBranches = resultSchema.GetProperty("oneOf").EnumerateArray().ToArray();
        await Assert.That(resultBranches).Count().IsEqualTo(2);
        var valueBranch = resultBranches.Single(branch =>
            branch.GetProperty("required").EnumerateArray().Any(value => value.GetString() == "value"));
        var errorBranch = resultBranches.Single(branch =>
            branch.GetProperty("required").EnumerateArray().Any(value => value.GetString() == "error"));
        await Assert.That(valueBranch.GetProperty("properties").GetProperty("value")
                .GetProperty("$ref").GetString())
            .EndsWith("/ForumDto");
        await Assert.That(errorBranch.GetProperty("properties").GetProperty("error")
                .GetProperty("$ref").GetString())
            .EndsWith("/ForumNotFoundError");

        var bookmarkedPostIdsSchema = paths
            .GetProperty("/api/posts/bookmarks/bulk/{postIds}")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(bookmarkedPostIdsSchema.GetProperty("items").GetProperty("$ref").GetString())
            .EndsWith("/PostId");

        var schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        await Assert.That(schemas.TryGetProperty("AuthenticationRequiredError", out _)).IsTrue();
        await Assert.That(schemas.TryGetProperty("ClaimNotFoundError", out _)).IsTrue();

        var createForumUnauthorizedSchema = paths
            .GetProperty("/api/forums")
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("401")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(GetReferencedSchemas(createForumUnauthorizedSchema, "oneOf"))
            .IsEquivalentTo(["AuthenticationRequiredError", "ClaimNotFoundError"]);

        var getForumUnauthorizedSchema = paths
            .GetProperty("/api/forums/{forumId}")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("401")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(getForumUnauthorizedSchema.GetProperty("$ref").GetString())
            .EndsWith("/ClaimNotFoundError");

        var threadDtoRequired = schemas.GetProperty("ThreadDto").GetProperty("required")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();
        await Assert.That(threadDtoRequired)
            .IsEquivalentTo(["threadId", "categoryId", "title", "createdBy", "createdAt", "state", "postCount"]);

        var forumParameters = paths.GetProperty("/api/forums").GetProperty("get")
            .GetProperty("parameters").EnumerateArray().ToArray();
        await Assert.That(GetParameterSchemaReference(forumParameters, "title")).EndsWith("/ForumTitle");
        await Assert.That(GetParameterSchemaReference(forumParameters, "createdBy")).EndsWith("/UserId");

        var searchParameters = paths.GetProperty("/api/search").GetProperty("get")
            .GetProperty("parameters").EnumerateArray().ToArray();
        await Assert.That(GetParameterSchemaReference(searchParameters, "cursor")).EndsWith("/SearchCursor");

        foreach (var path in paths.EnumerateObject())
            foreach (var method in path.Value.EnumerateObject().Where(property =>
                         property.Name is "get" or "post" or "put" or "patch" or "delete"))
            {
                var localeParameters = method.Value.GetProperty("parameters").EnumerateArray()
                    .Count(parameter =>
                        parameter.GetProperty("name").GetString() == HeaderNames.AcceptLanguage &&
                        parameter.GetProperty("in").GetString() == "header");
                await Assert.That(localeParameters).IsEqualTo(1);
            }

        var createForumResponses = paths.GetProperty("/api/forums").GetProperty("post")
            .GetProperty("responses");
        await Assert.That(createForumResponses.GetProperty("413").GetProperty("content")
                .GetProperty("application/problem+json").GetProperty("schema")
                .GetProperty("$ref").GetString())
            .EndsWith("/ApiProblemDetails");
        await Assert.That(createForumResponses.GetProperty("500").GetProperty("content")
                .GetProperty("application/problem+json").GetProperty("schema")
                .GetProperty("$ref").GetString())
            .EndsWith("/ApiProblemDetails");

        var statusSchema = problemDetails.GetProperty("properties").GetProperty("status");
        var statusTypes = statusSchema.GetProperty("type").EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();
        await Assert.That(statusTypes).IsEquivalentTo(["integer", "null"]);
        await Assert.That(statusSchema.TryGetProperty("pattern", out _)).IsFalse();

        var tags = json.RootElement.GetProperty("tags").EnumerateArray().ToArray();
        await Assert.That(tags.All(tag =>
                tag.TryGetProperty("description", out var description) &&
                !string.IsNullOrWhiteSpace(description.GetString())))
            .IsTrue();
    }

    [Test]
    public async Task OpenApi_UsesGeneratedRequestBindingMetadata(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.EnglishCode));
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var paths = json.RootElement.GetProperty("paths");
        var schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        await Assert.That(schemas.GetProperty("PaginationOffset").TryGetProperty("default", out _))
            .IsFalse();
        await Assert.That(schemas.GetProperty("PaginationLimitMin10Max100").TryGetProperty("default", out _))
            .IsFalse();
        await Assert.That(schemas.GetProperty("GetForumsPagedQuerySortType").TryGetProperty("default", out _))
            .IsFalse();

        var forumParameters = paths.GetProperty("/api/forums").GetProperty("get")
            .GetProperty("parameters").EnumerateArray().ToArray();

        var title = GetParameter(forumParameters, "title");
        await Assert.That(title.GetProperty("in").GetString()).IsEqualTo("query");
        await Assert.That(IsRequired(title)).IsFalse();
        await Assert.That(title.GetProperty("schema").GetProperty("$ref").GetString())
            .EndsWith("/ForumTitle");

        var offset = GetParameter(forumParameters, "offset");
        await Assert.That(IsRequired(offset)).IsFalse();
        var offsetSchema = offset.GetProperty("schema");
        if (!offsetSchema.TryGetProperty("default", out var offsetDefault))
            throw new InvalidOperationException(offset.GetRawText());
        await Assert.That(offsetDefault.GetInt32())
            .IsEqualTo(0);

        var limit = GetParameter(forumParameters, "limit");
        await Assert.That(IsRequired(limit)).IsFalse();
        await Assert.That(limit.GetProperty("schema").GetProperty("default").GetInt32())
            .IsEqualTo(100);

        var sort = GetParameter(forumParameters, "sort");
        await Assert.That(IsRequired(sort)).IsFalse();
        await Assert.That(sort.GetProperty("schema").GetProperty("default").GetString())
            .IsEqualTo("forumId");

        var threadCountParameters = paths.GetProperty("/api/threads/count").GetProperty("get")
            .GetProperty("parameters").EnumerateArray().ToArray();
        var status = GetParameter(threadCountParameters, "status");
        await Assert.That(IsRequired(status)).IsFalse();
        await Assert.That(threadCountParameters.Any(parameter =>
                parameter.GetProperty("name").GetString() == "state"))
            .IsFalse();

        var createPost = paths.GetProperty("/api/threads/{threadId}/posts").GetProperty("post");
        var threadId = GetParameter(
            createPost.GetProperty("parameters").EnumerateArray(),
            "threadId");
        await Assert.That(threadId.GetProperty("in").GetString()).IsEqualTo("path");
        await Assert.That(threadId.GetProperty("required").GetBoolean()).IsTrue();

        var requestBody = createPost.GetProperty("requestBody");
        await Assert.That(requestBody.GetProperty("required").GetBoolean()).IsTrue();
        await Assert.That(requestBody.GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema")
                .GetProperty("$ref")
                .GetString())
            .EndsWith("/CreatePostRequestBody");
    }

    private static string GetParameterSchemaReference(IEnumerable<JsonElement> parameters, string name) =>
        GetParameter(parameters, name)
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString()!;

    private static JsonElement GetParameter(IEnumerable<JsonElement> parameters, string name) =>
        parameters.Single(parameter => parameter.GetProperty("name").GetString() == name);

    private static bool IsRequired(JsonElement parameter) =>
        parameter.TryGetProperty("required", out var required) && required.GetBoolean();

    private static string[] GetReferencedSchemas(JsonElement schema, string compositionKeyword) =>
        schema.GetProperty(compositionKeyword)
            .EnumerateArray()
            .Select(branch => branch.GetProperty("$ref").GetString()!.Split('/')[^1])
            .ToArray();
}
