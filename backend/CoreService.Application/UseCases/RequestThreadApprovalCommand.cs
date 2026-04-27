using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    Success,
    ThreadNotFoundError,
    NonThreadOwnerError,
    ThreadNotInStateError,
    ThreadMustContainPostsError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class RequestThreadApprovalCommand : ICommand<CommandResult>
{
    public required UserId RequestedBy { get; init; }
}

public sealed class
    RequestThreadApprovalCommandHandler : ICommandHandler<RequestThreadApprovalCommand, CommandResult>
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestThreadApprovalCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(RequestThreadApprovalCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken)).ValueOrErrors(out var thread,
                out var error)) return error;

        if (thread.CreatedBy != command.RequestedBy) return new NonThreadOwnerError();

        if (!thread.RequestApproval().SuccessOrErrors(out var errors)) return errors.Value;

        await _unitOfWork.CommitAsync(cancellationToken);

        return Success.Instance;
    }
}