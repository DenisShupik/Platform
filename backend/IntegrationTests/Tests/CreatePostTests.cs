using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Xunit;
using CreateCategoryRequestBody = CoreService.Presentation.Rest.Dtos.CreateCategoryRequestBody;
using CreateForumRequestBody = CoreService.Presentation.Rest.Dtos.CreateForumRequestBody;
using CreatePostRequestBody = CoreService.Presentation.Rest.Dtos.CreatePostRequestBody;
using CreateThreadRequestBody = CoreService.Presentation.Rest.Dtos.CreateThreadRequestBody;

namespace IntegrationTests.Tests;

public sealed class CreatePostTests : IClassFixture<CoreServiceTestsFixture<CreatePostTests>>
{
    private readonly CoreServiceTestsFixture<CreatePostTests> _fixture;

    public CreatePostTests(CoreServiceTestsFixture<CreatePostTests> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ParallelCreatePosts_Success()
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

        var forumId =
            await client.CreateForumAsync(new CreateForumRequestBody
                {
                    Title = ForumTitle.From("Тестовый форум"),
                    AccessPolicyValue = PolicyValue.Any,
                    CategoryCreatePolicyValue = PolicyValue.Any,
                    ThreadCreatePolicyValue = PolicyValue.Any,
                    PostCreatePolicyValue = PolicyValue.Any
                },
                cancellationToken);

        var categoryId = await client.CreateCategoryAsync(new CreateCategoryRequestBody
            {
                ForumId = forumId,
                Title = CategoryTitle.From("Тестовый раздел"),
                AccessPolicyId = accessPolicyId,
                ThreadCreatePolicyId = threadCreatePolicyId,
                PostCreatePolicyId = postCreatePolicyId
            },
            cancellationToken);

        var threadId = await client.CreateThreadAsync(new CreateThreadRequestBody
            {
                CategoryId = categoryId,
                Title = ThreadTitle.From("Тестовая тема"),
                AccessPolicyId = accessPolicyId,
                PostCreatePolicyId = postCreatePolicyId
            },
            cancellationToken);

        var tasks = Enumerable.Range(0, 10).Select(x => client.CreatePostAsync(threadId,
            new CreatePostRequestBody { Content = PostContent.From($"Тестовое сообщение {x}") },
            cancellationToken));

        var postIds = await Task.WhenAll(tasks);
    }
}