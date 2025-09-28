using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetForumsPagedQuery<>), typeof(Forum))]
internal static partial class ForumReadRepositoryExtensions;

public sealed class ForumReadRepository : IForumReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ForumReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var projection = await _dbContext.Forums
            .Where(x => x.ForumId == id)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new ForumNotFoundError(id);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ForumId, Guid> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(e => ids.ToHashSet().Contains(e.ForumId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> request,
        CancellationToken cancellationToken)
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

        var forums = await query
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<ulong> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Forums
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy);

        var count = await query.LongCountAsyncLinqToDB(cancellationToken);

        return (ulong)count;
    }

    public async Task<Dictionary<ForumId, ulong>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
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

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);
    }
}