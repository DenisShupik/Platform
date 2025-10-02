using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CreateForumRequestBody = CoreService.Presentation.Rest.Dtos.CreateForumRequestBody;

namespace IntegrationTests.Tests;

public sealed class CreateForumTests : IClassFixture<CoreServiceTestsFixture<CreateForumTests>>
{
    private readonly CoreServiceTestsFixture<CreateForumTests> _fixture;

    public CreateForumTests(CoreServiceTestsFixture<CreateForumTests> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateForum_Success()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.GetCoreServiceClient(_fixture.TestUsername);

        var forumPolicySetId = await client.CreateForumPolicySetAsync(
            new CreateForumPolicySetRequestBody
            {
                Access = Policy.Any,
                CategoryCreate = Policy.Any,
                ThreadCreate = Policy.Any,
                PostCreate = Policy.Any,
            },
            cancellationToken);
        
        var request = new CreateForumRequestBody
        {
            Title = ForumTitle.From("Тестовый форум"),
            ForumPolicySetId = forumPolicySetId
        };

        var forumId = await client.CreateForumAsync(request, cancellationToken);

        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var forum = await dbContext.Forums
            .Include(forum => forum.Categories)
            .FirstOrDefaultAsync(x => x.ForumId == forumId, cancellationToken);
        Assert.NotNull(forum);
        Assert.Equal(request.Title, forum.Title);
        Assert.Empty(forum.Categories);
        Assert.Equal(_fixture.TestUserId, forum.CreatedBy);
    }
}