using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CreateCategoryRequestBody = CoreService.Presentation.Rest.Dtos.CreateCategoryRequestBody;
using CreateForumRequestBody = CoreService.Presentation.Rest.Dtos.CreateForumRequestBody;
using CreateThreadRequestBody = CoreService.Presentation.Rest.Dtos.CreateThreadRequestBody;

namespace IntegrationTests.Tests;

public sealed class CreateThreadTests : IClassFixture<CoreServiceTestsFixture<CreateThreadTests>>
{
    private readonly CoreServiceTestsFixture<CreateThreadTests> _fixture;

    public CreateThreadTests(CoreServiceTestsFixture<CreateThreadTests> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateThread_Success()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.GetCoreServiceClient(_fixture.TestUsername);

        var accessPolicyId = await client.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.Access,
                Value = PolicyValue.Any
            },
            cancellationToken);

        var categoryCreatePolicyId = await client.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.CategoryCreate,
                Value = PolicyValue.Any
            },
            cancellationToken);

        var threadCreatePolicyId = await client.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.ThreadCreate,
                Value = PolicyValue.Any
            },
            cancellationToken);

        var postCreatePolicyId = await client.CreatePolicyAsync(
            new CreatePolicyRequestBody
            {
                Type = PolicyType.PostCreate,
                Value = PolicyValue.Any
            },
            cancellationToken);

        var createForumRequestBody = new CreateForumRequestBody
        {
            Title = ForumTitle.From("Тестовый форум"),
            AccessPolicyId = accessPolicyId,
            CategoryCreatePolicyId = categoryCreatePolicyId,
            ThreadCreatePolicyId = threadCreatePolicyId,
            PostCreatePolicyId = postCreatePolicyId
        };

        var forumId = await client.CreateForumAsync(createForumRequestBody, cancellationToken);

        var createCategoryRequestBody = new CreateCategoryRequestBody
        {
            ForumId = forumId,
            Title = CategoryTitle.From("Тестовый раздел"),
            AccessPolicyId = accessPolicyId,
            ThreadCreatePolicyId = threadCreatePolicyId,
            PostCreatePolicyId = postCreatePolicyId
        };

        var categoryId = await client.CreateCategoryAsync(createCategoryRequestBody, cancellationToken);

        var createThreadRequestBody = new CreateThreadRequestBody
        {
            CategoryId = categoryId,
            Title = ThreadTitle.From("Новая тема"),
            AccessPolicyId = accessPolicyId,
            PostCreatePolicyId = postCreatePolicyId
        };

        var threadId = await client.CreateThreadAsync(createThreadRequestBody, cancellationToken);

        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var category = await dbContext.Threads
            .Include(e => e.Posts)
            .FirstOrDefaultAsync(x => x.ThreadId == threadId, cancellationToken);
        Assert.NotNull(category);
        Assert.Equal(createThreadRequestBody.Title, category.Title);
        Assert.Empty(category.Posts);
        Assert.Equal(_fixture.TestUserId, category.CreatedBy);
    }
}