using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreateCategoryCommandResult = Result<
    CategoryId,
    ForumNotFoundError,
    ForumAccessLevelError,
    ForumAccessRestrictedError
>;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.ForumId), nameof(Category.Title),
    nameof(Category.CreatedBy), nameof(Category.AccessLevel))]
public sealed partial class CreateCategoryCommand : ICommand<CreateCategoryCommandResult>;

public sealed class
    CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserWriteAccessAsync(command.CreatedBy, command.ForumId,
                cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<CategoryId>(out _, out var accessRestrictedError))
            return accessRestrictedError.Value;
        
        var forumOrError =
            await _forumWriteRepository.GetAsync<ForumCategoryAddable>(command.ForumId, cancellationToken);

        if (!forumOrError.TryPick(out var forum, out var error)) return error;

        var category = forum.AddCategory(command.Title, command.CreatedBy, DateTime.UtcNow, command.AccessLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }
}