using System.Net.Http.Json;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;

namespace SharedKernel.Tests.Services;

public sealed class CoreServiceClient
{
    private readonly HttpClient _httpClient;

    public CoreServiceClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    public async Task<ForumId> CreateForumAsync(CreateForumRequestBody body, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/forums", body, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumId>(cancellationToken);
    }

    public async Task<CategoryId> CreateCategoryAsync(CreateCategoryRequest requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/categories", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryId>(cancellationToken);
    }

    public async Task<ThreadId> CreateThreadAsync(CreateThreadRequest requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/threads", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ThreadId>(cancellationToken);
    }

    public async Task<PostId> CreatePostAsync(ThreadId threadId, CreatePostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync($"api/threads/{threadId}/posts", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostId>(cancellationToken);
    }
}