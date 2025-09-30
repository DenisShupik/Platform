using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Domain.Entities;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;

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

        if (projection == null) return new UserNotFoundError(userId);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<UserId, Guid> userIds,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Users
            .Where(x => userIds.ToHashSet().Contains(x.UserId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

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