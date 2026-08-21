using NotificationService.Application.Authorization;
using NotificationService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

using DeleteThreadSubscriptionCommandResult = SuccessOr<ThreadSubscriptionNotFoundError, PermissionDeniedError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class DeleteThreadSubscriptionCommand : ICommand<DeleteThreadSubscriptionCommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class
    DeleteThreadSubscriptionCommandHandler : ICommandHandler<DeleteThreadSubscriptionCommand,
    DeleteThreadSubscriptionCommandResult>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;
    private readonly IThreadSubscriptionPolicyEvaluator _policyEvaluator;

    public DeleteThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository,
        IThreadSubscriptionPolicyEvaluator policyEvaluator
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<DeleteThreadSubscriptionCommandResult> HandleAsync(DeleteThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var authorization = _policyEvaluator.Authorize(
            command.RequestedBy,
            ThreadSubscriptionPolicy.Manage,
            command.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var result = await _threadSubscriptionWriteRepository.ExecuteRemoveAsync(command.UserId, command.ThreadId,
            cancellationToken);
        if (result.TryGetFailure(out var failure)) return failure;

        return SuccessOr.Success;
    }
}
