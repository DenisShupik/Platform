using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using SharedKernel.Domain.ValueObjects;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;

namespace UserService.Infrastructure.Persistence.Repositories;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, UserNotFoundError>> GetByIdAsync<T>(UserId userId,
        CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError(userId);
        return projection;
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<UserId> userIds, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Users
            .Where(x => userIds.Contains(x.UserId))
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(int offset, int limit, CancellationToken cancellationToken)
    {
        IQueryable<User> query = _dbContext.Users.OrderBy(e => e.UserId);

        var projection = await query
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }
}