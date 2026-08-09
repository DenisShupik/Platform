using System.Net;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;
using Shared.Tests.Services;

namespace IntegrationTests.Tests;

public sealed class UserTokenServiceTests
{
    [Test]
    public async Task CancelledSemaphoreWait_DoesNotReleaseAnotherRequestLease()
    {
        var tokenHandler = new BlockingTokenHandler();
        using var tokenService = new UserTokenService(CreateOptions(), tokenHandler);

        var firstRequest = tokenService.GetAccessTokenAsync("user", CancellationToken.None);
        await tokenHandler.Entered.Task;

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var cancelledRequest = tokenService.GetAccessTokenAsync("user", cancellation.Token);
        tokenHandler.Release.TrySetResult();

        await Assert.That(async () => await cancelledRequest)
            .Throws<OperationCanceledException>();
        await Assert.That(await firstRequest).IsEqualTo("token-1");
        await Assert.That(await tokenService.GetAccessTokenAsync("user", CancellationToken.None))
            .IsEqualTo("token-1");
        await Assert.That(tokenHandler.RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task UnauthorizedResponse_InvalidatesTokenForExactSelectedUser()
    {
        var tokenHandler = new SequencedTokenHandler();
        using var tokenService = new UserTokenService(CreateOptions(), tokenHandler);
        var selectorCallCount = 0;
        using var client = new HttpClient(new UserTokenService.Handler(
            tokenService,
            () => selectorCallCount++ == 0 ? "selected-user" : "wrong-user")
        {
            InnerHandler = new UnauthorizedHandler()
        });

        using var response = await client.GetAsync("https://service.test/resource");
        var refreshedToken = await tokenService.GetAccessTokenAsync(
            "selected-user",
            CancellationToken.None);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(selectorCallCount).IsEqualTo(1);
        await Assert.That(refreshedToken).IsEqualTo("token-2");
        await Assert.That(tokenHandler.RequestCount).IsEqualTo(2);
    }

    private static IOptions<KeycloakOptions> CreateOptions() => Options.Create(new KeycloakOptions
    {
        Audience = "app-user",
        Issuer = "https://identity.test/realms/app",
        MetadataAddress = "https://identity.test/realms/app/.well-known/openid-configuration",
        Realm = "app",
        ServiceClientId = "service",
        ServiceClientSecret = "secret"
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
        private int _requestCount;

        public int RequestCount => Volatile.Read(ref _requestCount);
        public TaskCompletionSource Entered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestNumber = Interlocked.Increment(ref _requestCount);
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            return CreateTokenResponse($"token-{requestNumber}");
        }
    }

    private sealed class SequencedTokenHandler : HttpMessageHandler
    {
        private int _requestCount;

        public int RequestCount => Volatile.Read(ref _requestCount);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestNumber = Interlocked.Increment(ref _requestCount);
            return Task.FromResult(CreateTokenResponse($"token-{requestNumber}"));
        }
    }

    private sealed class UnauthorizedHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
    }
}
