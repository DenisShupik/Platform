using NotificationService.Application.Authorization;
using NotificationService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using CoreThreadNotFoundError = CoreService.Domain.Errors.ThreadNotFoundError;

namespace NotificationService.Application.UseCases;

using CreateThreadSubscriptionCommandResult =
    SuccessOr<DuplicateThreadSubscriptionError, NotificationService.Domain.Errors.PermissionDeniedError,
        CoreService.Domain.Errors.ThreadNotFoundError>;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class CreateThreadSubscriptionCommand : ICommand<CreateThreadSubscriptionCommandResult>
{
    public required EnumSet<ChannelType> Channels { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class
    CreateThreadSubscriptionCommandHandler : ICommandHandler<CreateThreadSubscriptionCommand,
    CreateThreadSubscriptionCommandResult>
{
    private readonly IThreadSubscriptionWriteRepository _threadSubscriptionWriteRepository;
    private readonly IThreadAccessReader _threadAccessReader;
    private readonly IThreadSubscriptionPolicyEvaluator _policyEvaluator;

    public CreateThreadSubscriptionCommandHandler(
        IThreadSubscriptionWriteRepository threadSubscriptionWriteRepository,
        IThreadAccessReader threadAccessReader,
        IThreadSubscriptionPolicyEvaluator policyEvaluator
    )
    {
        _threadSubscriptionWriteRepository = threadSubscriptionWriteRepository;
        _threadAccessReader = threadAccessReader;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<CreateThreadSubscriptionCommandResult> HandleAsync(
        CreateThreadSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var authorization = _policyEvaluator.Authorize(
            command.RequestedBy,
            ThreadSubscriptionPolicy.Manage,
            command.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var actorId = command.UserId == command.RequestedBy.UserId
            ? command.RequestedBy.UserId
            : command.UserId;
        if (!await _threadAccessReader.CanReadAsync(command.ThreadId, actorId, cancellationToken))
            return new CoreThreadNotFoundError();

        var addResult = await _threadSubscriptionWriteRepository.ExecuteAddAsync(
            new ThreadSubscription(command.UserId, command.ThreadId, command.Channels),
            cancellationToken);

        if (addResult.TryGetFailure(out var failure)) return failure;

        return SuccessOr.Success;
    }
}
