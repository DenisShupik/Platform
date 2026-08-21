using System.Data;
using CoreService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    ThreadNotFoundError,
    NonThreadOwnerError,
    ThreadNotInStateError,
    ThreadMustContainPostsError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class RequestThreadApprovalCommand : ICommand<CommandResult>
{
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RequestedAt { get; init; }
}

public sealed class
    RequestThreadApprovalCommandHandler : ICommandHandler<RequestThreadApprovalCommand, CommandResult>
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForumSanctionRepository _sanctions;

    public RequestThreadApprovalCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        IForumSanctionRepository sanctions,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _sanctions = sanctions;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(RequestThreadApprovalCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken)).TryGetValue(out var thread,
                out var error)) return error;

        if (thread.CreatedBy != command.RequestedBy.UserId) return new NonThreadOwnerError();

        if (await _sanctions.IsThreadParticipationRestrictedAsync(
                command.RequestedBy.UserId,
                command.ThreadId,
                command.RequestedAt,
                cancellationToken))
            return new PermissionDeniedError();

        if (thread.RequestApproval().TryGetFailure(out var failure)) return failure;

        await _unitOfWork.CommitAsync(cancellationToken);

        return SuccessOr.Success;
    }
}
