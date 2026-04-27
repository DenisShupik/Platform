using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Repositories;

public sealed class UserWriteRepository : IUserWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public UserWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<User, UserNotFoundError>> GetOneAsync(
        UserId userId,
        CancellationToken cancellationToken
    )
    {
        var projection = await _dbContext.Users
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (projection == null) return new UserNotFoundError();

        return projection;
    }
}