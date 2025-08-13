using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;
using UserService.Application.Interfaces;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Repositories;

public sealed class UserWriteRepository : IUserWriteRepository
{
    private readonly WritableApplicationDbContext _dbContext;

    public UserWriteRepository(WritableApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, UserNotFoundError>> GetByIdAsync<T>(
        UserId userId,
        CancellationToken cancellationToken
    )
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError(userId);
        return projection;
    }
}