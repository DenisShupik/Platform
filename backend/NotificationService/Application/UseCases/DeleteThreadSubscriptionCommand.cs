using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

using DeleteThreadSubscriptionCommandResult = Result<Success, ThreadSubscriptionNotFoundError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class
    DeleteThreadSubscriptionCommand : ICommand<DeleteThreadSubscriptionCommandResult>;

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

    public Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        return _threadSubscriptionWriteRepository.ExecuteRemoveAsync(command.UserId, command.ThreadId,
            cancellationToken);
    }
}