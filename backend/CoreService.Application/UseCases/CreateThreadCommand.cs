using System.Data;
using CoreService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CreateThreadCommandResult = Result<
    ThreadId,
    PermissionDeniedError,
    CategoryNotFoundError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title),
    nameof(Thread.CreatedAt))]
public sealed partial class CreateThreadCommand : ICreateCommand<CreateThreadCommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class
    CreateThreadCommandHandler : ICommandHandler<CreateThreadCommand, CreateThreadCommandResult>
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IForumSanctionRepository _sanctions;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        ICategoryWriteRepository categoryWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IForumSanctionRepository sanctions,
        IUnitOfWork unitOfWork
    )
    {
        _categoryWriteRepository = categoryWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _sanctions = sanctions;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var categoryResult =
            await _categoryWriteRepository.GetAsync(command.CategoryId, cancellationToken);

        if (!categoryResult.TryGetValue(out var category, out var error)) return error;

        var scope = AuthorizationScope.Category(category.ForumId, category.CategoryId);
        if (await _sanctions.IsParticipationRestrictedAsync(
                command.RequestedBy.UserId,
                scope,
                command.CreatedAt,
                cancellationToken))
            return new PermissionDeniedError();

        var thread = category.AddThread(command.Title, command.RequestedBy.UserId, command.CreatedAt);

        _threadWriteRepository.Add(thread);

        await _unitOfWork.CommitAsync(cancellationToken);

        return thread.ThreadId;
    }
}
