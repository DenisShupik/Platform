using System.Linq.Expressions;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Generator.Attributes;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Domain.Entities;
using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Repositories;

[AddApplySort(typeof(GetUsersPagedQuery.GetUsersPagedQuerySortType), typeof(User))]
internal static partial class UserReadRepositoryExtensions
{
    private static readonly Expression<Func<User, UserId>> UserIdExpression = e => e.UserId;
}

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public UserReadRepository(ReadApplicationDbContext dbContext)
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
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsync(cancellationToken);

        return projection;
    }
}