using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class DeleteThreadSubscriptionCommand;

[GenerateOneOf]
public partial class DeleteThreadSubscriptionCommandResult : OneOfBase<Success, ThreadSubscriptionNotFoundError>;

public sealed class DeleteThreadSubscriptionCommandHandler
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
    }

    public async Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var result =
            await _threadSubscriptionWriteRepository.ExecuteRemoveAsync(request.UserId, request.ThreadId, cancellationToken);
        return new DeleteThreadSubscriptionCommandResult(result);
    }
}