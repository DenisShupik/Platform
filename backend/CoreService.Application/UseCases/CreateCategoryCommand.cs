using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreateCategoryCommandResult = Result<
    CategoryId,
    PermissionDeniedError,
    ForumNotFoundError
>;

[Omit(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class CreateCategoryCommand : ICreateCommand<CreateCategoryCommandResult>
{
    public required Role CreatorRole { get; init; }
}

public sealed class
    CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IForumWriteRepository forumWriteRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CreatorRole < Role.Moderator) return new PermissionDeniedError();
        
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var forumOrError =
            await _forumWriteRepository.GetAsync<Forum>(command.ForumId, cancellationToken);

        if (!forumOrError.ValueOrErrors(out var forum, out var error)) return error;

        var category = forum.AddCategory(command.Title, command.CreatedBy, command.CreatedAt);
        
        _categoryWriteRepository.Add(category);

        await _unitOfWork.CommitAsync(cancellationToken);

        return category.CategoryId;
    }
}