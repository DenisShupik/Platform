using System.Net.Http.Json;
using CoreService.Domain.ValueObjects;
using NotificationService.Presentation.Apis.Dtos;

namespace SharedKernel.Tests.Services;

public sealed class NotificationServiceClient
{
    private readonly HttpClient _httpClient;

    public NotificationServiceClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }
    
    public async Task CreateThreadSubscriptionAsync(ThreadId threadId, CreateThreadSubscriptionRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync($"api/thread/{threadId}/subscriptions", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}