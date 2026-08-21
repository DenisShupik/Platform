using System.Data;
using CoreService.Application.Authorization;
using Shared.Domain.Abstractions.Results;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    ThreadNotFoundError,
    ThreadNotInStateError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class RejectThreadCommand : ICommand<CommandResult>
{
    public required DateTime RejectedAt { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class
    RejectThreadCommandHandler : ICommandHandler<RejectThreadCommand, CommandResult>
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IForumPolicyEvaluator _policies;
    private readonly IUnitOfWork _unitOfWork;

    public RejectThreadCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IForumPolicyEvaluator policies,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _policies = policies;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(RejectThreadCommand command, CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken)).TryGetValue(out var thread,
                out var error)) return error;

        var categoryResult = await _categoryWriteRepository.GetAsync(thread.CategoryId, cancellationToken);
        if (!categoryResult.TryGetValue(out var category)) return new ThreadNotFoundError();

        var authorization = await _policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.RejectThread,
            AuthorizationScope.Thread(category.ForumId, category.CategoryId, thread.ThreadId),
            command.RejectedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        if (thread.RejectThread().TryGetFailure(out var failure)) return failure;

        await _unitOfWork.PublishEventAsync(
            new ThreadRejectedEvent
            {
                ThreadId = thread.ThreadId,
                CreatedBy = thread.CreatedBy,
                RejectedBy = command.RequestedBy.UserId,
                RejectedAt = command.RejectedAt
            },
            cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return SuccessOr.Success;
    }
}
