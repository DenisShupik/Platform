using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;
using Xunit;

namespace IntegrationTests;

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

        var forumId =
            await client.CreateForumAsync(new CreateForumRequestBody { Title = ForumTitle.From("Тестовый форум") },
                cancellationToken);

        var categoryId = await client.CreateCategoryAsync(
            new CreateCategoryRequestBody { ForumId = forumId, Title = CategoryTitle.From("Тестовый раздел") },
            cancellationToken);

        var threadId = await client.CreateThreadAsync(
            new CreateThreadRequestBody { CategoryId = categoryId, Title = ThreadTitle.From("Тестовая тема") },
            cancellationToken);

        var tasks = Enumerable.Range(0, 10).Select(x => client.CreatePostAsync(threadId,
            new CreatePostRequestBody { Content = $"Тестовое сообщение {x}" },
            cancellationToken));

        var postIds = await Task.WhenAll(tasks);
    }
}