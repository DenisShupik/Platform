using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CategoryWriteRepository : ICategoryWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public CategoryWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Category, CategoryNotFoundError>> GetAsync(CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .Where(e => e.CategoryId == categoryId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (category == null) return new CategoryNotFoundError(categoryId);

        return category;
    }

    public void Add(Category category)
    {
        _dbContext.Categories.Add(category);
    }
}