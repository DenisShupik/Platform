using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Shared.Infrastructure.Persistence.Abstractions;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetUsersPagedQuery<>), typeof(User))]
internal static partial class UserReadRepositoryExtensions;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public UserReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, UserNotFoundError>> GetOneAsync<T>(UserId userId,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError();
        return projection;
    }

    public async Task<Dictionary<UserId, Result<T, UserNotFoundError>>> GetBulkAsync<T>(
        IdSet<UserId, Guid> userIds,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var ids = userIds.Select(x => x.Value).ToArray();

        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from p in _dbContext.Users
                    .Where(x => x.UserId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<Guid, User?>
                {
                    Key = id,
                    Value = p
                })
            .ProjectToType<SqlKeyValue<Guid, T?>>()
            .ToDictionaryAsyncLinqToDB(k => UserId.From(k.Key),
                k => (Result<T, UserNotFoundError>)(k.Value == null
                    ? new UserNotFoundError()
                    : k.Value), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        IQueryable<User> query = _dbContext.Users;

        var projection = await query
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }
}