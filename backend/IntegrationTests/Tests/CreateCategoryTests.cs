using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CreateCategoryRequestBody = CoreService.Presentation.Rest.Dtos.CreateCategoryRequestBody;
using CreateForumRequestBody = CoreService.Presentation.Rest.Dtos.CreateForumRequestBody;

namespace IntegrationTests.Tests;

public sealed class CreateCategoryTests : IClassFixture<CoreServiceTestsFixture<CreateCategoryTests>>
{
    private readonly CoreServiceTestsFixture<CreateCategoryTests> _fixture;

    public CreateCategoryTests(CoreServiceTestsFixture<CreateCategoryTests> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateCategory_Success()
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
            AccessPolicyValue = PolicyValue.Any,
            CategoryCreatePolicyValue = PolicyValue.Any,
            ThreadCreatePolicyValue = PolicyValue.Any,
            PostCreatePolicyValue = PolicyValue.Any
        };

        var forumId = await client.CreateForumAsync(createForumRequestBody, cancellationToken);

        var createCategoryRequestBody = new CreateCategoryRequestBody
        {
            ForumId = forumId,
            Title = CategoryTitle.From("Тестовый раздел"),
            AccessPolicyId = accessPolicyId,
            ThreadCreatePolicyId = threadCreatePolicyId,
            PostCreatePolicyId = postCreatePolicyId,
        };

        var categoryId = await client.CreateCategoryAsync(createCategoryRequestBody, cancellationToken);

        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var category = await dbContext.Categories
            .Include(e => e.Threads)
            .FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken);
        Assert.NotNull(category);
        Assert.Equal(createCategoryRequestBody.Title, category.Title);
        Assert.Empty(category.Threads);
        Assert.Equal(_fixture.TestUserId, category.CreatedBy);
    }
}