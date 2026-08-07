using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

using CreateThreadSubscriptionCommandResult = Result<Success, DuplicateThreadSubscriptionError, NotAdminError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand : ICommand<CreateThreadSubscriptionCommandResult>
{
    public required EnumSet<ChannelType> Channels { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class
    CreateThreadSubscriptionCommandHandler : ICommandHandler<CreateThreadSubscriptionCommand,
    CreateThreadSubscriptionCommandResult>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;

    public CreateThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
    }

    public async Task<CreateThreadSubscriptionCommandResult> HandleAsync(
        CreateThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId != command.RequestedBy.UserId && command.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var addResult = await _threadSubscriptionWriteRepository.ExecuteAddAsync(
            new ThreadSubscription(command.UserId, command.ThreadId, command.Channels),
            cancellationToken);

        return addResult.Match<CreateThreadSubscriptionCommandResult>(
            success => success,
            error => error
        );
    }
}
