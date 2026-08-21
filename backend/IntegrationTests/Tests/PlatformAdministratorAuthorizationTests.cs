using System.Net;
using CoreService.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class PlatformAdministratorAuthorizationTests
{
    [ClassDataSource<CoreServiceTestsFixture<PlatformAdministratorAuthorizationTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<PlatformAdministratorAuthorizationTests> Fixture { get; init; }

    [Test]
    public async Task Appointment_ChangesPoliciesWithoutIssuingAnotherToken(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var administratorActions = await administrator.GetPlatformAllowedActionsAsync(cancellationToken);
        await Assert.That(administratorActions.CanManageStructure).IsTrue();
        await Assert.That(administratorActions.CanManageAuthorization).IsTrue();

        var userActionsBefore = await user.GetPlatformAllowedActionsAsync(cancellationToken);
        await Assert.That(userActionsBefore.CanManageStructure).IsFalse();
        await Assert.That(userActionsBefore.CanManageAuthorization).IsFalse();

        using (var response = await user.GetPlatformAdministratorsResponseAsync(cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var initialAppointments = await administrator.GetPlatformAdministratorsAsync(cancellationToken);
        await Assert.That(initialAppointments).HasSingleItem();
        await Assert.That(initialAppointments[0].UserId).IsEqualTo(Fixture.TestModeratorUserId);
        await Assert.That(initialAppointments[0].GrantedBy).IsNull();
        await Assert.That(initialAppointments[0].WasBootstrapped).IsTrue();

        using (var response = await administrator.DeletePlatformAdministratorAsync(
                   Fixture.TestModeratorUserId,
                   cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        await administrator.AppointPlatformAdministratorAsync(Fixture.TestUserId, cancellationToken);

        var userActionsAfterAppointment = await user.GetPlatformAllowedActionsAsync(cancellationToken);
        await Assert.That(userActionsAfterAppointment.CanManageStructure).IsTrue();
        await Assert.That(userActionsAfterAppointment.CanManageAuthorization).IsTrue();

        var appointments = await user.GetPlatformAdministratorsAsync(cancellationToken);
        await Assert.That(appointments.Count).IsEqualTo(2);
        var appointedUser = appointments.Single(appointment => appointment.UserId == Fixture.TestUserId);
        await Assert.That(appointedUser.GrantedBy).IsEqualTo(Fixture.TestModeratorUserId);
        await Assert.That(appointedUser.WasBootstrapped).IsFalse();

        await user.RevokePlatformAdministratorAsync(Fixture.TestModeratorUserId, cancellationToken);

        var originalAdministratorActions = await administrator.GetPlatformAllowedActionsAsync(cancellationToken);
        await Assert.That(originalAdministratorActions.CanManageStructure).IsFalse();
        await Assert.That(originalAdministratorActions.CanManageAuthorization).IsFalse();

        using (var response = await user.DeletePlatformAdministratorAsync(Fixture.TestUserId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        Fixture.UserStatusReader.SetActive(Fixture.TestUserId, false);
        Fixture.UserStatusReader.SetActive(Fixture.TestModeratorUserId, true);

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<BootstrapPlatformAdministratorsCommandHandler>();
            var result = await handler.HandleAsync(
                new BootstrapPlatformAdministratorsCommand
                {
                    UserIds = [Fixture.TestModeratorUserId],
                    BootstrappedAt = DateTime.UtcNow
                },
                cancellationToken);
            await Assert.That(result.IsSuccess).IsTrue();
        }

        var recoveredAppointments = await administrator.GetPlatformAdministratorsAsync(cancellationToken);
        await Assert.That(recoveredAppointments.Select(appointment => appointment.UserId))
            .Contains(Fixture.TestModeratorUserId);

        using var lastReachableAdministrator = await administrator.DeletePlatformAdministratorAsync(
            Fixture.TestModeratorUserId,
            cancellationToken);
        await Assert.That(lastReachableAdministrator.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        Fixture.UserStatusReader.SetActive(Fixture.TestUserId, true);
    }
}
