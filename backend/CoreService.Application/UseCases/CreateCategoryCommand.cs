using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using OneOf;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.ForumId), nameof(Category.Title),
    nameof(Category.CreatedBy))]
public sealed partial class CreateCategoryCommand : ICommand<OneOf<CategoryId, ForumNotFoundError>>;

public sealed class
    CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, OneOf<CategoryId, ForumNotFoundError>>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<CategoryId, ForumNotFoundError>> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var forumOrError =
            await _forumWriteRepository.GetAsync<ForumCategoryAddable>(command.ForumId, cancellationToken);

        if (forumOrError.TryPickT1(out var error, out var forum)) return error;

        var category = forum.AddCategory(command.Title, command.CreatedBy, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }
}