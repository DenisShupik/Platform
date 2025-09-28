using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using OneOf;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class
    DeleteThreadSubscriptionCommand : ICommand<OneOf<Success, ThreadSubscriptionNotFoundError>>;

public sealed class
    DeleteThreadSubscriptionCommandHandler : ICommandHandler<DeleteThreadSubscriptionCommand,
    OneOf<Success, ThreadSubscriptionNotFoundError>>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
    }

    public Task<OneOf<Success, ThreadSubscriptionNotFoundError>> HandleAsync(DeleteThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        return _threadSubscriptionWriteRepository.ExecuteRemoveAsync(command.UserId, command.ThreadId,
            cancellationToken);
    }
}