using System.Net.Http.Json;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;

namespace DevEnv.Seeder;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:8000/api/");
    }

    public async Task<long> CreateForumAsync(CreateForumRequest requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("forums", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<long>(cancellationToken);
    }
    
    public async Task<long> CreateCategoryAsync(CreateCategoryRequest requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("categories", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<long>(cancellationToken);
    }
}