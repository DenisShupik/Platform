using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PolicyReadRepository : IPolicyReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PolicyReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<PolicyId, Result<T, PolicyNotFoundError>>> GetBulkAsync<T>(
        GetPoliciesBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull
    {
        var ids = query.PolicyIds.Select(x => x.Value).ToArray();
        var result = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from p in _dbContext.Policies.Where(e => e.PolicyId == id).DefaultIfEmpty()
                select new SqlKeyValue<Guid, Policy?>
                {
                    Key = id,
                    Value = p
                })
            .ProjectToType<SqlKeyValue<Guid, T?>>()
            .ToDictionaryAsyncLinqToDB(k => PolicyId.From(k.Key),
                k => (Result<T, PolicyNotFoundError>)(k.Value == null
                    ? new PolicyNotFoundError(PolicyId.From(k.Key))
                    : k.Value), cancellationToken);
        return result;
    }
}