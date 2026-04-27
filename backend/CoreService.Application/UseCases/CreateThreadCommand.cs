using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CreateThreadCommandResult = Result<
    ThreadId,
    PermissionDeniedError,
    CategoryNotFoundError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title),
    nameof(Thread.CreatedBy),
    nameof(Thread.CreatedAt))]
public sealed partial class CreateThreadCommand : ICreateCommand<CreateThreadCommandResult>
{
    public required Role CreatorRole { get; init; }
}

public sealed class
    CreateThreadCommandHandler : ICommandHandler<CreateThreadCommand, CreateThreadCommandResult>
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        ICategoryWriteRepository categoryWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryWriteRepository = categoryWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var categoryResult =
            await _categoryWriteRepository.GetAsync(command.CategoryId, cancellationToken);

        if (!categoryResult.ValueOrErrors(out var category, out var error)) return error;

        var thread = category.AddThread(command.Title, command.CreatedBy, command.CreatedAt);
        
        _threadWriteRepository.Add(thread);

        await _unitOfWork.CommitAsync(cancellationToken);

        return thread.ThreadId;
    }
}