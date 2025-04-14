using System.Net.Http.Json;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;

namespace DevEnv.Seeder.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:8000/api/");
    }

    private static HttpRequestMessage CreateRequest<T>(string url, T content, string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content != null ? JsonContent.Create(content) : null
        };
        request.Options.Set(new HttpRequestOptionsKey<string>("UserId"), userId);
        return request;
    }
    
    private static HttpRequestMessage UploadRequest(string url, HttpContent? content, string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        request.Options.Set(new HttpRequestOptionsKey<string>("UserId"), userId);
        return request;
    }

    public async Task<ForumId> CreateForumAsync(CreateForumRequest requestBody, string userId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest("forums", requestBody, userId);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ForumId>(cancellationToken);
    }

    public async Task<CategoryId> CreateCategoryAsync(CreateCategoryRequest requestBody, string userId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest("categories", requestBody, userId);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryId>(cancellationToken);
    }

    public async Task<ThreadId> CreateThreadAsync(CreateThreadRequest requestBody, string userId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest("threads", requestBody, userId);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ThreadId>(cancellationToken);
    }

    public async Task<PostId> CreatePostAsync(CreatePostRequest requestBody, string userId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest($"threads/{requestBody.ThreadId}/posts", requestBody.Body,
            userId);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PostId>(cancellationToken);
    }

    public async Task UploadAvatar(byte[] imageBytes, string userId, CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.Add("Content-Type", "image/webp");
        content.Add(fileContent, "file","user.webp");
        using var request = UploadRequest("avatars", content, userId);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}