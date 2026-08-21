using CoreService.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Persistence;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class NotificationAuthorizationFilteringTests
{
    [ClassDataSource<NotificationServiceTestsFixture<NotificationAuthorizationFilteringTests>>(
        Shared = SharedType.PerClass)]
    public required NotificationServiceTestsFixture<NotificationAuthorizationFilteringTests> Fixture { get; init; }

    [Test]
    public async Task Readability_IsAppliedBeforePaginationAndCount(CancellationToken cancellationToken)
    {
        var readableThreadId = ThreadId.From(Guid.NewGuid());
        var deniedThreadId = ThreadId.From(Guid.NewGuid());
        var now = DateTime.UtcNow;
        var events = Enumerable.Range(0, 11)
            .Select(index => new NotifiableEvent(
                new PostAddedNotifiableEventPayload(
                    index == 10 ? readableThreadId : deniedThreadId,
                    PostId.From(Guid.NewGuid()),
                    Fixture.TestUserId),
                now.AddMinutes(index)))
            .ToArray();

        using var scope = Fixture.Services.CreateScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<WriteApplicationDbContext>();
        writeDbContext.NotifiableEvents.AddRange(events);
        writeDbContext.Notifications.AddRange(events.Select(notifiableEvent =>
            new Notification(
                Fixture.TestUserId,
                notifiableEvent.NotifiableEventId,
                ChannelType.Internal)));
        await writeDbContext.SaveChangesAsync(cancellationToken);

        var repository = scope.ServiceProvider.GetRequiredService<INotificationReadRepository>();
        var query = CreateQuery(Fixture.TestUserId, PaginationOffset.Default);
        var readableThreadIds = new HashSet<ThreadId> { readableThreadId };

        var firstPage = await repository.GetAllAsync<InternalNotificationDto>(
            query,
            readableThreadIds,
            cancellationToken);
        var count = await repository.GetCountAsync(
            Fixture.TestUserId,
            null,
            ChannelType.Internal,
            readableThreadIds,
            cancellationToken);

        await Assert.That(firstPage.Items).HasSingleItem();
        await Assert.That(firstPage.TotalCount).IsEqualTo(Count.From(1));
        await Assert.That(count).IsEqualTo(Count.From(1));

        var emptyPage = await repository.GetAllAsync<InternalNotificationDto>(
            CreateQuery(Fixture.TestUserId, PaginationOffset.From(10)),
            readableThreadIds,
            cancellationToken);
        await Assert.That(emptyPage.Items).IsEmpty();
        await Assert.That(emptyPage.TotalCount).IsEqualTo(Count.From(1));
    }

    private static GetInternalNotificationsPagedQuery CreateQuery(
        UserId userId,
        PaginationOffset offset) =>
        new()
        {
            UserId = userId,
            IsDelivered = null,
            Offset = offset,
            Limit = PaginationLimit.From(10),
            Sort =
            [
                new SortCriteria<GetInternalNotificationsPagedQuerySortType>
                {
                    Field = GetInternalNotificationsPagedQuerySortType.OccurredAt,
                    Order = SortOrderType.Descending
                }
            ]
        };
}
