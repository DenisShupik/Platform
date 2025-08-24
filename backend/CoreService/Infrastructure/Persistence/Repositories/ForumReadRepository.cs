using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Infrastructure.Extensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumReadRepository : IForumReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ForumReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(x => x.ForumId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ForumNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ForumId> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(e => ids.ToHashSet().Contains(e.ForumId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Forum> query = _dbContext.Forums;

        if (request.Title != null)
        {
            query = query.Where(e =>
                e.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        if (request.CreatedBy != null)
        {
            query = query.Where(e => e.CreatedBy == request.CreatedBy.Value);
        }

        if (
            request.Sort is { Field: GetForumsPagedQuery.GetForumsPagedQuerySortType.ForumId } sort
        )
        {
            query = sort.Order == SortOrderType.Ascending
                ? query.OrderBy(e => e.ForumId)
                : query.OrderByDescending(e => e.ForumId);
        }

        var forums = await query
            .ProjectToType<T>()
            .ApplyPagination(request)
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<long> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Forums
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy);

        var count = await query.LongCountAsyncLinqToDB(cancellationToken);

        return count;
    }

    public async Task<Dictionary<ForumId, long>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken)
    {
        var forums = request.ForumIds.Select(x => x.Value).ToArray();
        var query =
            from f in _dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, forums)
            group c by f.ForumId
            into g
            select new { g.Key, Value = g.LongCount() };

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }
}