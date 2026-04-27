using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class GetCountTests
{
    [ClassDataSource<CoreServiceTestsFixture<GetCountTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<GetCountTests> Fixture { get; init; }

    [Test]
    public async Task GetForumsCount_ReturnsCorrectValue(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        
        // 1. Создаем несколько форумов
        await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);

        // 2. Вызов метода
        var count = await moderatorClient.GetForumsCountAsync(null, cancellationToken);

        // 3. Проверка (минимум 2, так как база может быть не пустой, но в TUnit обычно свежая база на класс)
        await Assert.That(count.Value).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetForumsCategoriesCount_ReturnsCorrectValues(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);

        // 1. Подготовка данных
        var forumId1 = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId1), cancellationToken);
        await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId1), cancellationToken);

        var forumId0 = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        
        var fakeForumId = ForumId.From(Guid.NewGuid());

        var forumIds = new IdSet<ForumId, Guid>([forumId1, forumId0, fakeForumId]);

        // 2. Вызов метода
        var result = await moderatorClient.GetForumsCategoriesCountAsync(forumIds, cancellationToken);

        // 3. Проверки
        await Assert.That(result.Count).IsEqualTo(3);

        // Форум с 2 категориями
        await result[forumId1].Match(
            async v => await Assert.That(v.Value).IsEqualTo(2),
            e => throw new Exception("Expected success for forumId1")
        );

        // Форум с 0 категориями
        await result[forumId0].Match(
            async v => await Assert.That(v.Value).IsEqualTo(0),
            e => throw new Exception("Expected success for forumId0")
        );

        // Несуществующий форум
        await result[fakeForumId].Match(
            v => throw new Exception("Expected failure for fakeForumId"),
            async e => await Assert.That(e).IsTypeOf<CoreService.Domain.Errors.ForumNotFoundError>()
        );
    }

    [Test]
    public async Task GetCategoriesThreadsCount_ReturnsCorrectValues(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        // 1. Подготовка данных
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId1 = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId1, "Thread 1"), cancellationToken);
        await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId1, "Thread 2"), cancellationToken);

        var categoryId0 = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        
        var fakeCategoryId = CategoryId.From(Guid.NewGuid());

        var categoryIds = new IdSet<CategoryId, Guid>([categoryId1, categoryId0, fakeCategoryId]);

        // 2. Вызов метода
        var result = await userClient.GetCategoriesThreadsCountAsync(categoryIds, null, cancellationToken);

        // 3. Проверки
        await Assert.That(result.Count).IsEqualTo(3);

       
        await Assert.That(result[categoryId1]).IsEqualTo(Count.From(2));
        await Assert.That(result[categoryId0]).IsEqualTo(Count.Default);
        
        await result[fakeCategoryId].Match(
            v => throw new Exception("Expected failure for fakeCategoryId"),
            async e => await Assert.That(e).IsTypeOf<CoreService.Domain.Errors.CategoryNotFoundError>()
        );
    }

    [Test]
    public async Task GetCategoriesPostsCount_ReturnsCorrectValues(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        // 1. Подготовка данных
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId1 = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId1 = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId1), cancellationToken);
        await userClient.CreatePostAsync(threadId1, TestRequests.CreatePost, cancellationToken);
        await userClient.CreatePostAsync(threadId1, TestRequests.CreatePost, cancellationToken);

        var categoryId0 = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        
        var fakeCategoryId = CategoryId.From(Guid.NewGuid());

        var categoryIds = new IdSet<CategoryId, Guid>([categoryId1, categoryId0, fakeCategoryId]);

        // 2. Вызов метода
        var result = await userClient.GetCategoriesPostsCountAsync(categoryIds, cancellationToken);

        // 3. Проверки
        await Assert.That(result.Count).IsEqualTo(3);

        await result[categoryId1].Match(
            async v => await Assert.That(v.Value).IsEqualTo(2),
            e => throw new Exception("Expected success for categoryId1")
        );

        await result[categoryId0].Match(
            async v => await Assert.That(v.Value).IsEqualTo(0),
            e => throw new Exception("Expected success for categoryId0")
        );

        await result[fakeCategoryId].Match(
            v => throw new Exception("Expected failure for fakeCategoryId"),
            async e => await Assert.That(e).IsTypeOf<CoreService.Domain.Errors.CategoryNotFoundError>()
        );
    }

    [Test]
    public async Task GetThreadsPostsCount_ReturnsCorrectValues(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        // 1. Подготовка данных
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        
        // Тема без постов (0 постов)
        var threadId0 = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId, "Thread 0"), cancellationToken);
        
        // Тема с 1 постом
        var threadId1 = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId, "Thread 1"), cancellationToken);
        await userClient.CreatePostAsync(threadId1, TestRequests.CreatePost, cancellationToken);
        
        // Несуществующая тема
        var fakeThreadId = ThreadId.From(Guid.NewGuid());

        var threadIds = new IdSet<ThreadId, Guid>([threadId0, threadId1, fakeThreadId]);

        // 2. Вызов метода
        var result = await userClient.GetThreadsPostsCountAsync(threadIds, null, cancellationToken);

        // 3. Проверки
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(3);

        // Проверка темы с 0 постов
        await Assert.That(result.ContainsKey(threadId0)).IsTrue();
        await result[threadId0].Match(
            async v => await Assert.That(v.Value).IsEqualTo(0),
            e => throw new Exception("Expected success for threadId0"),
            e => throw new Exception("Expected success for threadId0")
        );

        // Проверка темы с 1 постов
        await Assert.That(result.ContainsKey(threadId1)).IsTrue();
        await result[threadId1].Match(
            async v => await Assert.That(v.Value).IsEqualTo(1),
            e => throw new Exception("Expected success for threadId1"),
            e => throw new Exception("Expected success for threadId1")
        );

        // Проверка несуществующей темы (ошибка)
        await Assert.That(result.ContainsKey(fakeThreadId)).IsTrue();
        await result[fakeThreadId].Match(
            v => throw new Exception("Expected failure for fakeThreadId"),
            async e => await Assert.That(e).IsTypeOf<CoreService.Domain.Errors.ThreadNotFoundError>(),
            e => throw new Exception("Expected ThreadNotFoundError for fakeThreadId")
        );
    }

    [Test]
    public async Task GetThreadsCount_ReturnsCorrectValue(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        // 1. Создаем темы
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        // 2. Вызов метода
        var count = await userClient.GetThreadsCountAsync(null, null, cancellationToken);

        // 3. Проверка
        await Assert.That(count.Value).IsGreaterThanOrEqualTo(2);
    }
}
