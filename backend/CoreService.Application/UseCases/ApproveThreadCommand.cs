using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    Success,
    PermissionDeniedError,
    ThreadNotFoundError,
    ThreadNotInStateError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class ApproveThreadCommand : ICommand<CommandResult>
{
    public required UserId ApprovedBy { get; init; }
    public required DateTime ApprovedAt { get; init; }
    public required Role ApproverRole { get; init; }
}

public sealed class
    ApproveThreadCommandHandler : ICommandHandler<ApproveThreadCommand, CommandResult>
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveThreadCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(ApproveThreadCommand command, CancellationToken cancellationToken)
    {
        if (command.ApproverRole < Role.Moderator) return new PermissionDeniedError();

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken)).ValueOrErrors(out var thread,
                out var error)) return error;

        if (!thread.ApproveThread().SuccessOrErrors(out var error1)) return error1;

        await _unitOfWork.PublishEventAsync(
            new ThreadApprovedEvent
            {
                ThreadId = thread.ThreadId,
                CreatedBy = thread.CreatedBy,
                ApprovedBy = command.ApprovedBy,
                ApprovedAt = command.ApprovedAt
            },
            cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Success.Instance;
    }
}