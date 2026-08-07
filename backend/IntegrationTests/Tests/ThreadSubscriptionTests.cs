using System.Net;
using System.Net.Http.Json;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Presentation.Rest.Dtos;

namespace IntegrationTests.Tests;

public sealed class ThreadSubscriptionTests
{
    [ClassDataSource<NotificationServiceTestsFixture<ThreadSubscriptionTests>>(Shared = SharedType.PerClass)]
    public required NotificationServiceTestsFixture<ThreadSubscriptionTests> Fixture { get; init; }

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
