using System.Text.Json;
using CoreService.Application.Dtos;
using Shared.Domain.Abstractions.Results;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Extensions;

namespace Shared.Tests.Services;

public sealed class CoreServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CoreServiceClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(Locale.EnglishCode);
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .ApplyApiContractOptions()
            .ApplyCoreServiceOptions();
    }

    public async Task<ForumId> CreateForumAsync(CreateForumRequestBody requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/forums", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumId>(cancellationToken);
    }

    public Task<HttpResponseMessage> PostForumAsync(CreateForumRequestBody requestBody,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsJsonAsync("api/forums", requestBody, cancellationToken);

    public async Task<CategoryId> CreateCategoryAsync(CreateCategoryRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/categories", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryId>(cancellationToken);
    }

    public async Task AppointCategoryModeratorAsync(
        CategoryId categoryId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await PostAppointCategoryModeratorAsync(
            categoryId,
            userId,
            null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> PostAppointCategoryModeratorAsync(
        CategoryId categoryId,
        UserId userId,
        DateTime? validUntil,
        CancellationToken cancellationToken)
    {
        var path = $"api/categories/{categoryId}/moderators/{userId}";
        if (validUntil is not null)
            path += $"?validUntil={Uri.EscapeDataString(validUntil.Value.ToString("O"))}";

        return _httpClient.PostAsync(path, null, cancellationToken);
    }

    public async Task RevokeCategoryModeratorAsync(
        CategoryId categoryId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/categories/{categoryId}/moderators/{userId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CategoryAllowedActionsDto> GetCategoryAllowedActionsAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"api/categories/{categoryId}/allowed-actions",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryAllowedActionsDto>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryModeratorAppointmentDto>> GetCategoryModeratorsAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        using var response = await GetCategoryModeratorsResponseAsync(categoryId, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryModeratorAppointmentDto>>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public Task<HttpResponseMessage> GetCategoryModeratorsResponseAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken) =>
        _httpClient.GetAsync($"api/categories/{categoryId}/moderators", cancellationToken);

    public async Task AppointForumModeratorAsync(
        ForumId forumId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(
            $"api/forums/{forumId}/moderators/{userId}",
            null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RevokeForumModeratorAsync(
        ForumId forumId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/forums/{forumId}/moderators/{userId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<ForumModeratorAppointmentDto>> GetForumModeratorsAsync(
        ForumId forumId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"api/forums/{forumId}/moderators", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<ForumModeratorAppointmentDto>>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public async Task<CapabilityGrantId> GrantCapabilityAsync(
        GrantCapabilityRequestBody body,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/authorization/grants",
            body,
            _jsonSerializerOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CapabilityGrantId>(cancellationToken);
    }

    public Task<HttpResponseMessage> PostGrantCapabilityAsync(
        GrantCapabilityRequestBody body,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsJsonAsync(
            "api/authorization/grants",
            body,
            _jsonSerializerOptions,
            cancellationToken);

    public async Task RevokeCapabilityAsync(
        CapabilityGrantId capabilityGrantId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/authorization/grants/{capabilityGrantId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<CapabilityGrantDto>> GetCapabilityGrantsAsync(
        AuthorizationScopeType scopeType,
        ForumId? forumId,
        CategoryId? categoryId,
        ThreadId? threadId,
        CancellationToken cancellationToken)
    {
        var query = new List<string> { $"scopeType={scopeType}" };
        if (forumId is not null) query.Add($"forumId={forumId}");
        if (categoryId is not null) query.Add($"categoryId={categoryId}");
        if (threadId is not null) query.Add($"threadId={threadId}");

        using var response = await _httpClient.GetAsync(
            $"api/authorization/grants?{string.Join('&', query)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CapabilityGrantDto>>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public async Task<ForumSanctionId> IssueForumSanctionAsync(
        IssueForumSanctionRequestBody body,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/authorization/sanctions",
            body,
            _jsonSerializerOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumSanctionId>(cancellationToken);
    }

    public Task<HttpResponseMessage> PostIssueForumSanctionAsync(
        IssueForumSanctionRequestBody body,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsJsonAsync(
            "api/authorization/sanctions",
            body,
            _jsonSerializerOptions,
            cancellationToken);

    public async Task RevokeForumSanctionAsync(
        ForumSanctionId forumSanctionId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/authorization/sanctions/{forumSanctionId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> GetThreadResponseAsync(
        ThreadId threadId,
        CancellationToken cancellationToken) =>
        _httpClient.GetAsync($"api/threads/{threadId}", cancellationToken);

    public Task<HttpResponseMessage> GetForumResponseAsync(
        ForumId forumId,
        CancellationToken cancellationToken) =>
        _httpClient.GetAsync($"api/forums/{forumId}", cancellationToken);

    public Task<HttpResponseMessage> GetCategoryResponseAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken) =>
        _httpClient.GetAsync($"api/categories/{categoryId}", cancellationToken);

    public Task<HttpResponseMessage> PostCreatePostAsync(
        ThreadId threadId,
        CreatePostRequestBody body,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsJsonAsync(
            $"api/threads/{threadId}/posts",
            body,
            _jsonSerializerOptions,
            cancellationToken);

    public async Task<PlatformAllowedActionsDto> GetPlatformAllowedActionsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            "api/authorization/platform/allowed-actions",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformAllowedActionsDto>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformAdministratorAppointmentDto>> GetPlatformAdministratorsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            "api/authorization/platform/administrators",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PlatformAdministratorAppointmentDto>>(
            _jsonSerializerOptions,
            cancellationToken);
    }

    public Task<HttpResponseMessage> GetPlatformAdministratorsResponseAsync(
        CancellationToken cancellationToken) =>
        _httpClient.GetAsync("api/authorization/platform/administrators", cancellationToken);

    public async Task AppointPlatformAdministratorAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await PostAppointPlatformAdministratorAsync(userId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> PostAppointPlatformAdministratorAsync(
        UserId userId,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsync($"api/authorization/platform/administrators/{userId}", null, cancellationToken);

    public async Task RevokePlatformAdministratorAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await DeletePlatformAdministratorAsync(userId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> DeletePlatformAdministratorAsync(
        UserId userId,
        CancellationToken cancellationToken) =>
        _httpClient.DeleteAsync($"api/authorization/platform/administrators/{userId}", cancellationToken);

    public async Task<ThreadId> CreateThreadAsync(CreateThreadRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/threads", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ThreadId>(cancellationToken);
    }

    public async Task RequestThreadApprovalAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsync($"api/threads/{threadId}/request-approval", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ApproveThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync($"api/threads/{threadId}/approve", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync($"api/threads/{threadId}/reject", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PostDto> GetPostAsync(PostId postId, CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.GetAsync($"api/posts/{postId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<PostDto>> GetThreadPostsAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"api/threads/{threadId}/posts", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PostDto>>(cancellationToken);
    }

    public async Task<PostId> CreatePostAsync(ThreadId threadId, CreatePostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync($"api/threads/{threadId}/posts", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostId>(cancellationToken);
    }

    public async Task CreatePostBookmarkAsync(PostId postId, CancellationToken cancellationToken)
    {
        using var response = await PostBookmarkAsync(postId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> PostApproveThreadAsync(
        ThreadId threadId,
        CancellationToken cancellationToken) =>
        _httpClient.PostAsync($"api/threads/{threadId}/approve", null, cancellationToken);

    public Task<HttpResponseMessage> PostBookmarkAsync(PostId postId, CancellationToken cancellationToken)
    {
        return _httpClient.PostAsync($"api/posts/bookmarks/{postId}", null, cancellationToken);
    }

    public async Task<SearchResultsDto> SearchAsync(
        SearchTerm term,
        SearchResultType? type,
        SortCriteria<SearchQuerySortType> sort,
        SearchCursor? cursor,
        CancellationToken cancellationToken,
        PaginationOffset? offset = null)
    {
        var sortValue = sort.Order == SortOrderType.Descending
            ? $"-{sort.Field}"
            : sort.Field.ToString();
        var url = $"api/search?term={Uri.EscapeDataString(term.Value)}&sort={sortValue}";
        if (type is { } resultType) url += $"&type={resultType}";
        if (cursor is { } value) url += $"&cursor={Uri.EscapeDataString(value.Value)}";
        if (offset is { } offsetValue) url += $"&offset={offsetValue.Value}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Error: {response.StatusCode}, Content: {content}", null,
                response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SearchResultsDto>(_jsonSerializerOptions, cancellationToken))!;
    }

    public Task<SearchResultsDto> SearchAsync(
        SearchTerm term,
        SearchResultType? type,
        SortCriteria<SearchQuerySortType> sort,
        CancellationToken cancellationToken) =>
        SearchAsync(term, type, sort, null, cancellationToken);

    public async Task UpdatePostAsync(PostId postId, UpdatePostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PatchAsJsonAsync($"api/posts/{postId}", requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Error: {response.StatusCode}, Content: {content}", null,
                response.StatusCode);
        }
    }

    public async Task DeletePostAsync(PostId postId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync($"api/posts/{postId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Error: {response.StatusCode}, Content: {content}", null,
                response.StatusCode);
        }
    }

    public async Task<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>
        GetThreadsPostsCountAsync(
            IdSet<ThreadId, Guid> threadIds,
            CoreService.Domain.Enums.ThreadState? status,
            CancellationToken cancellationToken)
    {
        var ids = string.Join(",", threadIds);
        var url = $"api/threads/{ids}/posts/count";
        if (status.HasValue)
        {
            url += $"?status={status}";
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Error: {response.StatusCode}, Content: {content}", null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>(
                _jsonSerializerOptions, cancellationToken);
    }

    public async Task<Count> GetForumsCountAsync(UserId? createdBy, CancellationToken cancellationToken)
    {
        var url = "api/forums/count";
        if (createdBy != null) url += $"?createdBy={createdBy}";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Count>(cancellationToken);
    }

    public async Task<Dictionary<ForumId, Result<Count, ForumNotFoundError>>> GetForumsCategoriesCountAsync(
        IdSet<ForumId, Guid> forumIds, CancellationToken cancellationToken)
    {
        var ids = string.Join(",", forumIds);
        using var response = await _httpClient.GetAsync($"api/forums/{ids}/categories/count", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<ForumId, Result<Count, ForumNotFoundError>>>(
            _jsonSerializerOptions, cancellationToken);
    }

    public async Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesPostsCountAsync(
        IdSet<CategoryId, Guid> categoryIds, CancellationToken cancellationToken)
    {
        var ids = string.Join(",", categoryIds);
        using var response = await _httpClient.GetAsync($"api/categories/{ids}/posts/count", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>>(
            _jsonSerializerOptions, cancellationToken);
    }

    public async Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesThreadsCountAsync(
        IdSet<CategoryId, Guid> categoryIds, CoreService.Domain.Enums.ThreadState? state,
        CancellationToken cancellationToken)
    {
        var ids = string.Join(",", categoryIds);
        var url = $"api/categories/{ids}/threads/count";
        if (state.HasValue) url += $"?state={state}";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>>(
            _jsonSerializerOptions, cancellationToken);
    }

    public async Task<Count> GetThreadsCountAsync(UserId? createdBy, CoreService.Domain.Enums.ThreadState? status,
        CancellationToken cancellationToken)
    {
        var url = "api/threads/count";
        var queryParams = new List<string>();
        if (createdBy != null) queryParams.Add($"createdBy={createdBy}");
        if (status != null) queryParams.Add($"status={status}");
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Count>(cancellationToken);
    }
}
