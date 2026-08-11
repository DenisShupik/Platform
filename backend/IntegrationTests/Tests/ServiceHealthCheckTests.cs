using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Shared.Presentation.Extensions;

namespace IntegrationTests.Tests;

public sealed class ServiceHealthCheckTests
{
    [Test]
    public async Task HealthEndpoints_ExposeOnlyLiveAndReady(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddServiceHealthChecks();

        await using var application = builder.Build();
        application.MapServiceHealthChecks();
        await application.StartAsync(cancellationToken);

        using var client = application.GetTestClient();
        using var livenessResponse = await client.GetAsync(
            ServiceHealthCheckExtensions.LivenessPath,
            cancellationToken);
        using var readinessResponse = await client.GetAsync(
            ServiceHealthCheckExtensions.ReadinessPath,
            cancellationToken);
        using var legacyResponse = await client.GetAsync("/health", cancellationToken);

        await Assert.That(livenessResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(readinessResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(legacyResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
