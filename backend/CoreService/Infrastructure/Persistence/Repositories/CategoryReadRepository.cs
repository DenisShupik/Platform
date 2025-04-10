using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, CategoryNotFoundError>> GetByIdAsync<T>(CategoryId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(x => x.CategoryId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new CategoryNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<CategoryId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Categories
            .Where(x => ids.Contains(x.CategoryId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = await _dbContext.Categories
            .OrderBy(e => e.CategoryId)
            .Where(x => request.ForumId == null || x.ForumId == request.ForumId)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return query;
    }
}