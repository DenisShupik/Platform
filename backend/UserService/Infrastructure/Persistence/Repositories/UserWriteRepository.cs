using LinqToDB.EntityFrameworkCore;
using OneOf;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Repositories;

public sealed class UserWriteRepository : IUserWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public UserWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<User, UserNotFoundError>> GetOneAsync(
        UserId userId,
        CancellationToken cancellationToken
    )
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError(userId);

        return projection;
    }
}