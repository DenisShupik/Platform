using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryWriteRepository : ICategoryWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public CategoryWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, CategoryNotFoundError>> GetAsync<T>(CategoryId categoryId,
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