using System.Net;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class ThreadSubscriptionTests
{
    [ClassDataSource<NotificationServiceTestsFixture<ThreadSubscriptionTests>>(Shared = SharedType.PerClass)]
    public required NotificationServiceTestsFixture<ThreadSubscriptionTests> Fixture { get; init; }

    [Test]
    public async Task LatestEvents_TranslatePayloadPredicate_AndReturnLatestPostEvent(
        CancellationToken cancellationToken)
    {
        var threadId = ThreadId.From(Guid.NewGuid());
        var occurredAt = DateTime.UtcNow;
        var postAdded = new NotifiableEvent(
            new PostAddedNotifiableEventPayload(threadId, PostId.From(Guid.NewGuid()), Fixture.TestUserId),
            occurredAt.AddMinutes(-2));
        var postUpdated = new NotifiableEvent(
            new PostUpdatedNotifiableEventPayload(threadId, PostId.From(Guid.NewGuid()), Fixture.TestUserId),
            occurredAt.AddMinutes(-1));
        var threadApproved = new NotifiableEvent(
            new ThreadApprovedNotifiableEventPayload(
                threadId,
                Fixture.TestUserId,
                Fixture.TestUserId,
                occurredAt),
            occurredAt);

        using var scope = Fixture.Services.CreateScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<WriteApplicationDbContext>();
        writeDbContext.ThreadSubscriptions.Add(new ThreadSubscription(
            Fixture.TestUserId,
            threadId,
            new EnumSet<ChannelType>([ChannelType.Internal])));
        writeDbContext.NotifiableEvents.AddRange(postAdded, postUpdated, threadApproved);
        await writeDbContext.SaveChangesAsync(cancellationToken);

        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationWriteRepository>();
        await notificationRepository.BulkAddAsync(
            postUpdated.NotifiableEventId,
            threadId,
            UserId.From(Guid.NewGuid()),
            cancellationToken);

        var readDbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var notificationChannels = await readDbContext.Notifications
            .Where(notification => notification.NotifiableEventId == postUpdated.NotifiableEventId &&
                                   notification.UserId == Fixture.TestUserId)
            .Select(notification => notification.Channel)
            .ToListAsync(cancellationToken);
        var repository = scope.ServiceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();
        var events = await repository.GetLatestEventsAsync<ThreadSubscriptionLatestEventDto>(
            new GetThreadSubscriptionLatestEventsPagedQuery<ThreadSubscriptionLatestEventDto>
            {
                UserId = Fixture.TestUserId,
                RequestedBy = new UserIdRole(Fixture.TestUserId, Role.User),
                Offset = PaginationOffset.Default,
                Limit = PaginationLimit.From(100),
                Sort = new SortCriteria<GetThreadSubscriptionLatestEventsPagedQuerySortType>
                {
                    Field = GetThreadSubscriptionLatestEventsPagedQuerySortType.LatestEvent,
                    Order = SortOrderType.Descending
                }
            },
            cancellationToken);

        await Assert.That(events).HasSingleItem();
        await Assert.That(events[0].NotifiableEventId).IsEqualTo(postUpdated.NotifiableEventId);
        await Assert.That(events[0].Payload is PostUpdatedNotifiableEventPayload).IsTrue();
        await Assert.That(notificationChannels).IsEquivalentTo([ChannelType.Internal]);
    }

    [Test]
    public async Task CreateSubscription_FailsWithDuplicateError_When_AlreadyExists(CancellationToken cancellationToken)
    {
        var client = Fixture.GetNotificationServiceClient(Fixture.TestUsername);
        var threadId = ThreadId.From(Guid.NewGuid());
        var request = new CreateThreadSubscriptionRequestBody
        {
            Channels = [ChannelType.Internal, ChannelType.Email]
        };

        await client.CreateThreadSubscriptionAsync(Fixture.TestUserId, threadId, request, cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var efSubscription = await dbContext.ThreadSubscriptions
            .SingleAsync(subscription => subscription.UserId == Fixture.TestUserId
                                         && subscription.ThreadId == threadId,
                cancellationToken);
        var linqToDbSubscription = await dbContext.ThreadSubscriptions
            .Where(subscription => subscription.UserId == Fixture.TestUserId
                                   && subscription.ThreadId == threadId)
            .SingleAsyncLinqToDB(cancellationToken);

        await Assert.That(efSubscription.Channels).IsEquivalentTo(request.Channels);
        await Assert.That(linqToDbSubscription.Channels).IsEquivalentTo(request.Channels);

        using var response =
            await client.PostThreadSubscriptionAsync(Fixture.TestUserId, threadId, request, cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        var error = await response.Content.ReadFromJsonAsync<DuplicateThreadSubscriptionError>(cancellationToken);
        await Assert.That(error).IsEqualTo(new DuplicateThreadSubscriptionError(Fixture.TestUserId, threadId));
    }
}
