using System.Net.Http.Json;
using CoreService.Domain.ValueObjects;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Domain.ValueObjects;

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

    public async Task CreateThreadSubscriptionAsync(UserId userId, ThreadId threadId,
        CreateThreadSubscriptionRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await PostThreadSubscriptionAsync(userId, threadId, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> PostThreadSubscriptionAsync(UserId userId, ThreadId threadId,
        CreateThreadSubscriptionRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        return _httpClient.PostAsJsonAsync($"api/users/{userId}/subscriptions/{threadId}", requestBody,
            cancellationToken);
    }
}
