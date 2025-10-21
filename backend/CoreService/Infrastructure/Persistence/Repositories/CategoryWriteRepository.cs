using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryWriteRepository : ICategoryWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public CategoryWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CategoryThreadAddable, CategoryNotFoundError>> GetAsync(CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var category = await _dbContext.Set<CategoryThreadAddable>()
            .Where(e => e.CategoryId == categoryId)
            .Include(e => e.ReadPolicy)
            .Include(e => e.PostCreatePolicy)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (category == null) return new CategoryNotFoundError(categoryId);

        return category;
    }
}