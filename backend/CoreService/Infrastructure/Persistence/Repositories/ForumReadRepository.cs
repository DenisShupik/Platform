using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Shared.Infrastructure.Persistence.Abstractions;

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

    public async Task<Result<T, ForumNotFoundError>> GetOneAsync<T>(
        GetForumQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await _dbContext.Forums
            .Where(e => e.ForumId == query.ForumId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ForumNotFoundError();

        return result;
    }

    public async Task<Dictionary<ForumId, Result<T, ForumNotFoundError>>> GetBulkAsync<T>(
        GetForumsBulkQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var ids = query.ForumIds.Select(x => x.Value).ToArray();
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from f in _dbContext.Forums
                    .Where(e => e.ForumId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<Guid, Forum?>
                {
                    Key = id,
                    Value = f
                })
            .ProjectToType<SqlKeyValue<Guid, T?>>()
            .ToDictionaryAsyncLinqToDB(k => ForumId.From(k.Key),
                k => (Result<T, ForumNotFoundError>)(k.Value == null
                    ? new ForumNotFoundError()
                    : k.Value), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Forums.AsQueryable();
        if (query.Title != null)
        {
            queryable = queryable.Where(e =>
                e.Title.ToSqlString()
                    .Contains(query.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        var forums = await queryable
            .Where(e => query.CreatedBy == null || e.CreatedBy == query.CreatedBy)
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<Count> GetCountAsync(GetForumsCountQuery query, CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Forums
            .Where(e => query.CreatedBy == null || e.CreatedBy == query.CreatedBy);

        var count = await queryable.CountAsyncLinqToDB(cancellationToken);

        return Count.From(count);
    }

    public async Task<Dictionary<ForumId, Result<Count, ForumNotFoundError>>> GetForumsCategoriesCountAsync(
        GetForumsCategoriesCountQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.ForumIds.Select(e => e.Value).ToArray();

        var forums =
            from id in _dbContext.ToTvcLinqToDb(ids)
            from f in _dbContext.Forums
                .Where(e => e.ForumId == id)
                .DefaultIfEmpty()
            select new
            {
                ForumId = id,
                IsExists = f != null
            };

        var result = await (
                from f in forums
                from c in _dbContext.Categories
                    .Where(e => e.ForumId == f.ForumId)
                    .DefaultIfEmpty()
                group c by f
                into g
                select new { g.Key, Value = g.CountExt(e => e.CategoryId) })
            .ToDictionaryAsyncLinqToDB(k => ForumId.From(k.Key.ForumId),
                v => (Result<Count, ForumNotFoundError>)(!v.Key.IsExists
                    ? new ForumNotFoundError()
                    : Count.From(v.Value)), cancellationToken);

        return result;
    }
}