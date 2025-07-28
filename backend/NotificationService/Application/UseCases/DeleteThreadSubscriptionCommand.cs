using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;

namespace NotificationService.Application.UseCases;

[IncludeAsRequired(typeof(ThreadSubscription), nameof(ThreadSubscription.UserId), nameof(ThreadSubscription.ThreadId))]
public sealed partial class DeleteThreadSubscriptionCommand;

[GenerateOneOf]
public partial class DeleteThreadSubscriptionCommandResult : OneOfBase<Success, ThreadSubscriptionNotFoundError>;

public sealed class DeleteThreadSubscriptionCommandHandler
{
    private readonly IThreadSubscriptionRepository _threadSubscriptionRepository;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionRepository threadSubscriptionRepository
    )
    {
        _threadSubscriptionRepository = threadSubscriptionRepository;
    }

    public async Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _threadSubscriptionRepository.RemoveAsync(request.UserId, request.ThreadId, cancellationToken))
        {
            return new ThreadSubscriptionNotFoundError(request.UserId, request.ThreadId);
        }

        return OneOfHelper.Success;
    }
}