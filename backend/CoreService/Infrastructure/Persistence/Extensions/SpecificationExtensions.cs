using System.Linq.Expressions;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using LinqToDB;
using UserService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Extensions;

public static class SpecificationExtensions
{
    public static IQueryable<ProjectionWithAccessInfo<T>> OnlyAvailable<T>(
        this IQueryable<ProjectionWithAccessInfo<T>> queryable, UserId? queriedBy) =>
        queryable
            .Where(e => queriedBy == null
                ? e.ReadPolicyValue == PolicyValue.Any
                : !e.HasRestriction && (e.ReadPolicyValue < PolicyValue.Granted || e.HasGrant));

    [ExpressionMethod(nameof(IsRestrictionActiveExpression))]
    public static bool IsActive(this Restriction restriction, DateTime evaluatedAt)
        => throw new ServerSideOnlyException(nameof(IsActive));

    public static Expression<Func<Restriction, DateTime, bool>> IsRestrictionActiveExpression()
        => (restriction, evaluatedAt) => restriction.ExpiredAt == null || restriction.ExpiredAt.Value > evaluatedAt;

    [ExpressionMethod(nameof(GetPortalRestrictionExpression))]
    public static IQueryable<PortalRestriction> GetPortalRestriction(this ApplicationDbContext dbContext, UserId userId,
        PolicyType policy, DateTime evaluatedAt)
        => throw new ServerSideOnlyException(nameof(GetPortalRestriction));

    public static Expression<
            Func<ApplicationDbContext, UserId, PolicyType, DateTime, IQueryable<PortalRestriction?>>>
        GetPortalRestrictionExpression()
        => (dbContext, userId, policy, evaluatedAt) => dbContext.PortalRestrictions
            .Where(e => e.UserId == userId && e.Type == policy && e.IsActive(evaluatedAt));

    [ExpressionMethod(nameof(GetForumRestrictionExpression))]
    public static IQueryable<ForumRestriction> GetForumRestriction(this ApplicationDbContext dbContext, UserId userId,
        ForumId forumId, PolicyType policy, DateTime evaluatedAt)
        => throw new ServerSideOnlyException(nameof(GetForumRestriction));

    public static Expression<
            Func<ApplicationDbContext, UserId, ForumId, PolicyType, DateTime, IQueryable<ForumRestriction?>>>
        GetForumRestrictionExpression()
        => (dbContext, userId, forumId, policy, evaluatedAt) => dbContext.ForumRestrictions
            .Where(e => e.UserId == userId && e.ForumId == forumId && e.Type == policy && e.IsActive(evaluatedAt));

    [ExpressionMethod(nameof(GetCategoryRestrictionExpression))]
    public static IQueryable<CategoryRestriction?> GetCategoryRestriction(this ApplicationDbContext dbContext,
        UserId userId,
        CategoryId categoryId, PolicyType policy, DateTime evaluatedAt)
        => throw new ServerSideOnlyException(nameof(GetCategoryRestriction));

    public static Expression<
            Func<ApplicationDbContext, UserId, CategoryId, PolicyType, DateTime, IQueryable<CategoryRestriction?>>>
        GetCategoryRestrictionExpression()
        => (dbContext, userId, categoryId, policy, evaluatedAt) => dbContext.CategoryRestrictions
            .Where(e => e.UserId == userId && e.CategoryId == categoryId && e.Type == policy &&
                        e.IsActive(evaluatedAt));

    [ExpressionMethod(nameof(GetThreadRestrictionExpression))]
    public static IQueryable<ThreadRestriction?> GetThreadRestriction(this ApplicationDbContext dbContext,
        UserId userId,
        ThreadId threadId, PolicyType policy, DateTime evaluatedAt)
        => throw new ServerSideOnlyException(nameof(GetThreadRestriction));

    public static Expression<
            Func<ApplicationDbContext, UserId, ThreadId, PolicyType, DateTime, IQueryable<ThreadRestriction?>>>
        GetThreadRestrictionExpression()
        => (dbContext, userId, threadId, policy, evaluatedAt) => dbContext.ThreadRestrictions
            .Where(e => e.UserId == userId && e.ThreadId == threadId && e.Type == policy && e.IsActive(evaluatedAt));

    [ExpressionMethod(nameof(IsPolicySatisfiedExpression))]
    public static bool IsPolicySatisfied(this ApplicationDbContext dbContext, Policy policy, UserId userId)
        => throw new ServerSideOnlyException(nameof(IsPolicySatisfied));

    public static Expression<Func<ApplicationDbContext, Policy, UserId, bool>> IsPolicySatisfiedExpression()
        => (dbContext, policy, userId) =>
            policy.Value != PolicyValue.Granted ||
            dbContext.Grants.Any(y => y.PolicyId == policy.PolicyId && y.UserId == userId);

    [ExpressionMethod(nameof(IsPolicySatisfiedForAnonymousExpression))]
    public static bool IsPolicySatisfied(this ApplicationDbContext dbContext, Policy policy, UserId? userId)
        => throw new ServerSideOnlyException(nameof(IsPolicySatisfied));

    public static Expression<Func<ApplicationDbContext, Policy, UserId?, bool>>
        IsPolicySatisfiedForAnonymousExpression()
        => (dbContext, policy, userId) => userId == null
            ? policy.Value == PolicyValue.Any
            : policy.Value != PolicyValue.Granted ||
              dbContext.Grants.Any(y => y.PolicyId == policy.PolicyId && y.UserId == userId);
}