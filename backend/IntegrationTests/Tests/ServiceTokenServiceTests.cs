using System.Net;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;

namespace IntegrationTests.Tests;

public sealed class ServiceTokenServiceTests
{
    [Test]
    public async Task CancelledSemaphoreWait_DoesNotReleaseAnotherRequestLease()
    {
        var tokenHandler = new BlockingTokenHandler();
        using var tokenService = new ServiceTokenService(CreateOptions(), CreateServiceAccountOptions(), tokenHandler);
        using var client = CreateAuthorizedClient(tokenService, new SuccessfulHandler());

        var firstRequest = client.GetAsync("https://service.test/first");
        await tokenHandler.Entered.Task;

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Exception? cancellationException = null;
        try
        {
            await client.GetAsync("https://service.test/second", cancellation.Token);
        }
        catch (Exception exception)
        {
            cancellationException = exception;
        }

        tokenHandler.Release.TrySetResult();
        using var firstResponse = await firstRequest;

        await Assert.That(cancellationException).IsTypeOf<OperationCanceledException>();
        await Assert.That(firstResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task UnauthorizedResponse_InvalidatesCachedTokenForNextAttempt()
    {
        var tokenHandler = new SequencedTokenHandler();
        var targetHandler = new UnauthorizedThenSuccessHandler();
        using var tokenService = new ServiceTokenService(CreateOptions(), CreateServiceAccountOptions(), tokenHandler);
        using var client = CreateAuthorizedClient(tokenService, targetHandler);

        using var firstResponse = await client.GetAsync("https://service.test/first");
        using var secondResponse = await client.GetAsync("https://service.test/second");

        await Assert.That(firstResponse.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(secondResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(tokenHandler.RequestCount).IsEqualTo(2);
        await Assert.That(targetHandler.Tokens).IsEquivalentTo(["token-1", "token-2"]);
    }

    private static HttpClient CreateAuthorizedClient(
        ServiceTokenService tokenService,
        HttpMessageHandler innerHandler) =>
        new(new ServiceTokenService.Handler(tokenService) { InnerHandler = innerHandler });

    private static IOptions<KeycloakOptions> CreateOptions() => Options.Create(new KeycloakOptions
    {
        Audience = "app",
        InternalAudience = "app-internal",
        Issuer = "https://identity.test/realms/app",
        MetadataAddress = "https://identity.test/realms/app/.well-known/openid-configuration",
        Realm = "app"
    });

    private static IOptions<ServiceAccountOptions> CreateServiceAccountOptions() =>
        Options.Create(new ServiceAccountOptions
        {
            ClientId = "service",
            ClientSecret = "secret"
        });

    private static HttpResponseMessage CreateTokenResponse(string token) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(
            $$"""{"access_token":"{{token}}","expires_in":300}""",
            System.Text.Encoding.UTF8,
            "application/json")
    };

    private sealed class BlockingTokenHandler : HttpMessageHandler
    {
        public TaskCompletionSource Entered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            return CreateTokenResponse("token");
        }
    }

    private sealed class SequencedTokenHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(CreateTokenResponse($"token-{RequestCount}"));
        }
    }

    private sealed class SuccessfulHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class UnauthorizedThenSuccessHandler : HttpMessageHandler
    {
        public List<string> Tokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Tokens.Add(request.Headers.Authorization?.Parameter ?? string.Empty);
            var statusCode = Tokens.Count == 1
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
