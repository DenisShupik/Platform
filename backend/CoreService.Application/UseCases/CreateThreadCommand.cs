using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CreateThreadCommandResult = Result<ThreadId, CategoryNotFoundError>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.CategoryId), nameof(Thread.Title),
    nameof(Thread.CreatedBy), nameof(Thread.AccessLevel), nameof(Thread.Policies))]
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
        // var accessCheckResult =
        //     await _accessRestrictionReadRepository.CheckUserWriteAccessAsync(command.CreatedBy, command.CategoryId,
        //         cancellationToken);
        //
        // if (!accessCheckResult.TryGetOrMap<CategoryId>(out _, out var accessRestrictedError))
        //     return accessRestrictedError.Value;

        var categoryOrError =
            await _categoryWriteRepository.GetAsync<CategoryThreadAddable>(command.CategoryId, cancellationToken);

        if (!categoryOrError.TryGet(out var category, out var error)) return error;

        var thread = category.AddThread(command.Title, command.CreatedBy, DateTime.UtcNow, command.AccessLevel,
            command.Policies);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return thread.ThreadId;
    }
}