using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using OneOf;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Domain.Entities;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Repositories;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly ReadonlyApplicationDbContext _dbContext;

    public UserReadRepository(ReadonlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, UserNotFoundError>> GetOneAsync<T>(UserId userId,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError(userId);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(HashSet<UserId> userIds, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Users
            .Where(x => userIds.Contains(x.UserId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        IQueryable<User> query = _dbContext.Users.OrderBy(e => e.UserId);

        var projection = await query
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }
}