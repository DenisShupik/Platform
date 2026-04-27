using System.Net.Http.Json;
using System.Text.Json;
using CoreService.Application.Dtos;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Rest.Dtos;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

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
        _jsonSerializerOptions = new JsonSerializerOptions().ApplyCoreServiceOptions();
    }

    public async Task<ForumId> CreateForumAsync(CreateForumRequestBody requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/forums", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumId>(cancellationToken);
    }

    public async Task<CategoryId> CreateCategoryAsync(CreateCategoryRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/categories", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryId>(cancellationToken);
    }

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
