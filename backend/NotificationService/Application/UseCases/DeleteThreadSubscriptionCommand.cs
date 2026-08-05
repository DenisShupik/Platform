using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

using DeleteThreadSubscriptionCommandResult = Result<Success, ThreadSubscriptionNotFoundError, NotAdminError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class DeleteThreadSubscriptionCommand : ICommand<DeleteThreadSubscriptionCommandResult>
{
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class
    DeleteThreadSubscriptionCommandHandler : ICommandHandler<DeleteThreadSubscriptionCommand,
    DeleteThreadSubscriptionCommandResult>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
    }

    public async Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId != command.RequestedBy.UserId && command.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var result = await _threadSubscriptionWriteRepository.ExecuteRemoveAsync(command.UserId, command.ThreadId,
            cancellationToken);
        return result.Match<DeleteThreadSubscriptionCommandResult>(
            success => success,
            error => error
        );
    }
}
