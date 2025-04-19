using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, CategoryNotFoundError>> GetAsync<T>(CategoryId categoryId,
        CancellationToken cancellationToken)
        where T : class, IHasCategoryId
    {
        var forum = await _dbContext.Set<T>()
            .Where(e => e.CategoryId == categoryId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (forum == null) return new CategoryNotFoundError(categoryId);

        return forum;
    }
}