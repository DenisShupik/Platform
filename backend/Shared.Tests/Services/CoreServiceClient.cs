using System.Net.Http.Json;
using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;

namespace Shared.Tests.Services;

public sealed class CoreServiceClient
{
    private readonly HttpClient _httpClient;

    public CoreServiceClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    public async Task<ForumId> CreateForumAsync(CreateForumRequestBody requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/forums", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumId>(cancellationToken);
    }

    public async Task<PolicyId> CreatePolicyAsync(CreatePolicyRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/forum_policy_sets", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PolicyId>(cancellationToken);
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

    public async Task<PostDto> GetPostAsync(PostId postId, CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.GetAsync($"api/posts/{postId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostDto>(cancellationToken);
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
        response.EnsureSuccessStatusCode();
    }
}