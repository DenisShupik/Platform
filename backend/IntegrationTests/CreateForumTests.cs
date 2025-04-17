using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Presentation.Options;
using SharedKernel.Tests.Dtos;
using SharedKernel.Tests.Services;
using Xunit;

namespace IntegrationTests;

public sealed class CreateForumTestsFixture : WebApplicationFactory<Program>
{
    public readonly InfrastructureFixture InfrastructureFixture;
    public readonly string TestUsername = "test_user";
    public UserId TestUserId;
    
    public CreateForumTestsFixture(
        InfrastructureFixture fixture
    )
    {
        InfrastructureFixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder =
            InfrastructureFixture.CreateDatabase($"{nameof(CreateForumTests).ToLower()}_platform_db");

        builder.UseEnvironment("Development");
        builder.UseSetting("KeycloakOptions:MetadataAddress",
            InfrastructureFixture.KeycloakOptions.MetadataAddress);
        builder.UseSetting("KeycloakOptions:Issuer", InfrastructureFixture.KeycloakOptions.Issuer);
        builder.UseSetting("KeycloakOptions:Audience", InfrastructureFixture.KeycloakOptions.Audience);
        builder.UseSetting("KeycloakOptions:Realm", InfrastructureFixture.KeycloakOptions.Realm);
        builder.UseSetting("CoreServiceOptions:ConnectionString", connectionStringBuilder.ConnectionString);

        var httpClientHandler = new HttpClientHandler();
        var serviceTokenHandler = new ServiceTokenService.Handler(InfrastructureFixture.ServiceTokenService)
        {
            InnerHandler = httpClientHandler
        };
        using var httpClient = new HttpClient(serviceTokenHandler);

        var keycloakAdminClient =
            new KeycloakAdminClient(httpClient, new OptionsWrapper<KeycloakOptions>(InfrastructureFixture.KeycloakOptions));

        TestUserId = keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
        {
            Username = TestUsername,
            FirstName = "Иван",
            LastName = "Иванов",
            Email = $"{TestUsername}@app.com",
            Enabled = true,
            Credentials =
            [
                new()
                {
                    Type = "password",
                    Value = "12345678",
                    Temporary = false
                }
            ]
        }, CancellationToken.None).Result;
        
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
            var handler = new UserTokenService.Handler(InfrastructureFixture.UserTokenService, () => username);
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
        var client = _fixture.GetCoreServiceClient(_fixture.TestUsername);

        var request = new CreateForumRequest { Title = ForumTitle.From("Тестовый форум") };

        var forumId = await client.CreateForumAsync(request, cancellationToken);

        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var forum = await dbContext.Forums
            .Include(forum => forum.Categories)
            .FirstOrDefaultAsync(x => x.ForumId == forumId, cancellationToken);
        Assert.NotNull(forum);
        Assert.Equal(request.Title, forum.Title);
        Assert.Empty(forum.Categories);
        Assert.Equal(_fixture.TestUserId, forum.CreatedBy);
    }
}