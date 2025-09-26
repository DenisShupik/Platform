using System.Net.Http.Json;
using NotificationService.Presentation.Rest.Dtos;

namespace Shared.Tests.Services;

public sealed class NotificationServiceClient
{
    private readonly HttpClient _httpClient;

    public NotificationServiceClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    public async Task CreateThreadSubscriptionAsync(CreateThreadSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync($"api/thread/{request.ThreadId}/subscriptions", request.Body,
                cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}