using System.Linq.Expressions;
using LinqToDB;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Infrastructure.Persistence.Extensions;

public static class SpecificationExtensions
{
    [ExpressionMethod(nameof(CanReadThreadImpl))]
    public static bool CanReadThread(this Thread thread, UserIdRole? actor)
        => throw new InvalidOperationException("This method should not be called directly");

    private static Expression<Func<Thread, UserIdRole?, bool>> CanReadThreadImpl() =>
        (thread, userIdRole) =>
            userIdRole == null
                ? thread.State == ThreadState.Approved
                : userIdRole.Value.Role > Role.User ||
                  thread.State == ThreadState.Approved ||
                  thread.CreatedBy == userIdRole.Value.UserId;
    
    // [ExpressionMethod(nameof(HasEffectiveForumPermissionImpl))]
    // public static bool HasEffectivePermissionLinqToDB(this ApplicationDbContext dbContext, RoleId roleId,
    //     PermissionType type, ForumId forumId)
    //     => throw new InvalidOperationException("This method should not be called directly");
    //
    // private static Expression<Func<ApplicationDbContext, RoleId, PermissionType, ForumId, bool>>
    //     HasEffectiveForumPermissionImpl() =>
    //     (dbContext, roleId, type, forumId) =>
    //         dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             p.Scope == PermissionScope.Global &&
    //             p.Allow)
    //         &&
    //         !dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             !p.Allow &&
    //             (p.Scope == PermissionScope.Forum && ((ForumPermission)p).ForumId == forumId)
    //         );
    //
    // [ExpressionMethod(nameof(HasEffectiveCategoryPermissionImpl))]
    // public static bool HasEffectivePermissionLinqToDB(this ApplicationDbContext dbContext, RoleId roleId,
    //     PermissionType type,
    //     ForumId forumId, CategoryId categoryId)
    //     => throw new InvalidOperationException("This method should not be called directly");
    //
    // private static Expression<Func<ApplicationDbContext, RoleId, PermissionType, ForumId, CategoryId, bool>>
    //     HasEffectiveCategoryPermissionImpl() =>
    //     (dbContext, roleId, type, forumId, categoryId) =>
    //         dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             p.Scope == PermissionScope.Global &&
    //             p.Allow)
    //         &&
    //         !dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             !p.Allow &&
    //             (
    //                 (p.Scope == PermissionScope.Forum && ((ForumPermission)p).ForumId == forumId) ||
    //                 (p.Scope == PermissionScope.Category && ((CategoryPermission)p).CategoryId == categoryId)
    //             ));
    //
    // [ExpressionMethod(nameof(HasEffectiveThreadPermissionImpl))]
    // public static bool HasEffectivePermissionLinqToDB(this ApplicationDbContext dbContext, RoleId roleId,
    //     PermissionType type, ForumId forumId, CategoryId categoryId, ThreadId threadId)
    //     => throw new InvalidOperationException("This method should not be called directly");
    //
    // private static Expression<Func<ApplicationDbContext, RoleId, PermissionType, ForumId, CategoryId, ThreadId, bool>>
    //     HasEffectiveThreadPermissionImpl() =>
    //     (dbContext, roleId, type, forumId, categoryId, threadId) =>
    //         dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             p.Allow)
    //         &&
    //         !dbContext.Permissions.Any(p =>
    //             p.RoleId == roleId &&
    //             p.Type == type &&
    //             !p.Allow &&
    //             (
    //                 (p.Scope == PermissionScope.Forum && ((ForumPermission)p).ForumId == forumId) ||
    //                 (p.Scope == PermissionScope.Category && ((CategoryPermission)p).CategoryId == categoryId) ||
    //                 (p.Scope == PermissionScope.Thread && ((ThreadPermission)p).ThreadId == threadId)
    //             ));
    //
    // public static IQueryable<RoleId> QueryRoleId(this ApplicationDbContext dbContext, UserId? userId)
    //     => userId == null
    //         ? dbContext.Roles
    //             .Where(e => e.Title == "guest")
    //             .Select(e => e.RoleId)
    //         : dbContext.UserRoles
    //             .Where(e => e.UserId == userId)
    //             .Select(e => e.RoleId);
    //
    //
    // public static IQueryable<ProjectionWithAccess<Thread>> GetAvailableThreadsLinq2DB(
    //     this ApplicationDbContext dbContext, UserId? queriedBy, ThreadState? state, UserId? createdBy
    // ) =>
    //     from roleId in dbContext.QueryRoleId(queriedBy)
    //     from t in dbContext.Threads
    //     join c in dbContext.Categories on t.CategoryId equals c.CategoryId
    //     where state == null || t.State == state
    //     where createdBy == null || t.CreatedBy == createdBy
    //     select new ProjectionWithAccess<Thread>
    //     {
    //         Projection = t,
    //         HasAccess =
    //             dbContext.HasEffectivePermissionLinqToDB(roleId, PermissionType.Read, c.ForumId, c.CategoryId,
    //                 t.ThreadId) &&
    //             (
    //                 state == ThreadState.Published ||
    //                 t.State == ThreadState.Published ||
    //                 t.CreatedBy == queriedBy ||
    //                 dbContext.HasEffectivePermissionLinqToDB(
    //                     roleId, PermissionType.ApproveThread, c.ForumId, c.CategoryId, t.ThreadId)
    //             )
    //     };
}