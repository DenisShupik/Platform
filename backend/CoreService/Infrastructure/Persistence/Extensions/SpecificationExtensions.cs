using CoreService.Domain.Enums;
using CoreService.Infrastructure.Persistence.Abstractions;
using Mapster;
using UserService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Extensions;

public static class SpecificationExtensions
{
    public static IQueryable<ProjectionWithAccessInfo<T>> OnlyAvailable<T>(
        this IQueryable<ProjectionWithAccessInfo<T>> queryable, UserId? queriedBy) =>
        queryable
            .Where(e => queriedBy == null
                ? e.AccessPolicyValue == PolicyValue.Any
                : !e.HasRestriction && (e.AccessPolicyValue < PolicyValue.Granted || e.HasGrant));

    public static IQueryable<T> Flat<T, P>(this IQueryable<ProjectionWithAccessInfo<P>> queryable) =>
        queryable
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .Select(e => e.Projection);
}