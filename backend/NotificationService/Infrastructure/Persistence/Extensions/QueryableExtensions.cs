using CoreService.Domain.ValueObjects;
using LinqToDB;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Extensions;

public static class QueryableExtensions
{
    [Sql.Expression("({0}->>'$type' IN ('PostAdded','PostUpdated')) AND {0}->>'ThreadId'={1}::text", ServerSideOnly = true, IgnoreGenericParameters = true)]
    public static bool TestQ([ExprParameter] this NotifiableEventPayload input, [ExprParameter] ThreadId threadId) =>
        throw new ServerSideOnlyException(nameof(TestQ));
}

