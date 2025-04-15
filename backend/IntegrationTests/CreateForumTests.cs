using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SharedKernel.Tests.Services;
using Xunit;

namespace IntegrationTests;

public sealed class CreateForumTestsFixture : WebApplicationFactory<Program>
{
    private readonly InfrastructureFixture _infrastructureFixture;

    public CreateForumTestsFixture(
        InfrastructureFixture fixture
    )
    {
        _infrastructureFixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder =
            _infrastructureFixture.CreateDatabase($"{nameof(CreateForumTests).ToLower()}_platform_db");

        builder.UseEnvironment("Development");
        builder.UseSetting("KeycloakOptions:MetadataAddress",
            _infrastructureFixture.KeycloakOptions.MetadataAddress);
        builder.UseSetting("KeycloakOptions:Issuer", _infrastructureFixture.KeycloakOptions.Issuer);
        builder.UseSetting("KeycloakOptions:Audience", _infrastructureFixture.KeycloakOptions.Audience);
        builder.UseSetting("KeycloakOptions:Realm", _infrastructureFixture.KeycloakOptions.Realm);
        builder.UseSetting("CoreServiceOptions:ConnectionString", connectionStringBuilder.ConnectionString);
    }

    public CoreServiceClient GetCoreServiceClient(string? username = null)
    {
        HttpClient client;
        if (username == null)
        {
            client = CreateClient();
        }
        else
        {
            var handler = new UserTokenService.Handler(_infrastructureFixture.UserTokenService, () => username);
            client = CreateDefaultClient(handler);
        }

        return new CoreServiceClient(client);
    }
}

public sealed class CreateForumTests : IClassFixture<CreateForumTestsFixture>
{
    private readonly CreateForumTestsFixture _fixture;

    public CreateForumTests(CreateForumTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateForum_Success()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.GetCoreServiceClient("user_test");
        var forumId =
            await client.CreateForumAsync(
                new CreateForumRequest { Title = ForumTitle.From("Тестовый форум") }, cancellationToken);

        // using var scope = _factory.Services.CreateScope();
        // var client = _factory.ClientOptions.BaseAddress;
        // var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}