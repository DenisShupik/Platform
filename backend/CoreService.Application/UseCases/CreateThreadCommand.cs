using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CreateThreadCommandResult =
    Result<ThreadId, CategoryNotFoundError, ForumAccessPolicyViolationError, ForumPolicyRestrictedError,
        CategoryAccessPolicyViolationError, CategoryPolicyRestrictedError, ThreadCreatePolicyViolationError>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title),
    nameof(Thread.CreatedBy), nameof(Thread.ThreadPolicySetId))]
public sealed partial class CreateThreadCommand : ICommand<CreateThreadCommandResult>;

public sealed class
    CreateThreadCommandHandler : ICommandHandler<CreateThreadCommand, CreateThreadCommandResult>
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryWriteRepository = categoryWriteRepository;
        _unitOfWork = unitOfWork;
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
    }

    public async Task<CreateThreadCommandResult> HandleAsync(CreateThreadCommand command,
        CancellationToken cancellationToken)
    {
        var canCreateResult =
            await _accessRestrictionReadRepository.CanUserCanCreateThreadAsync(command.CreatedBy, command.CategoryId,
                cancellationToken);

        if (!canCreateResult.TryOrMapError<ThreadId>(out var accessRestrictedError))
            return accessRestrictedError.Value;

        var categoryResult =
            await _categoryWriteRepository.GetAsync<CategoryThreadAddable>(command.CategoryId, cancellationToken);

        if (!categoryResult.TryGet(out var category, out var error)) return error;

        var thread = category.AddThread(command.Title, command.CreatedBy, DateTime.UtcNow, command.ThreadPolicySetId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}