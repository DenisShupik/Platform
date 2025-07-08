using CoreService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace NotificationService.Infrastructure.Services;

public sealed class CoreServiceClient
{
    private readonly HttpClient _httpClient;

    public CoreServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://core-service:8010/api/");
    }

    public async Task<Thread> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"threads/{threadId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Thread>(cancellationToken);
    }
}