using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Tests;

public sealed class CreateCategoryTests
{
    [ClassDataSource<CoreServiceTestsFixture<CreateCategoryTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<CreateCategoryTests> Fixture { get; init; }

    [Test]
    public async Task CreateCategory_Success(CancellationToken cancellationToken)
    {
        var client = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);

        var forumId = await client.CreateForumAsync(TestRequests.CreateForum, cancellationToken);

        var categoryId = await client.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken);
        await Assert.That(category).IsNotNull();
        await Assert.That(category.Title).IsEqualTo(TestRequests.CreateCategory(forumId).Title);
        await Assert.That(category.CreatedBy).IsEqualTo(Fixture.TestModeratorUserId);
    }
}