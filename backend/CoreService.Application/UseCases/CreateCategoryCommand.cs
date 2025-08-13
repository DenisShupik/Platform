using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;
using OneOf;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.ForumId), nameof(Category.Title),
    nameof(Category.CreatedBy))]
public sealed partial class CreateCategoryCommand;

[GenerateOneOf]
public partial class CreateCategoryCommandResult : OneOfBase<CategoryId, ForumNotFoundError>;

public sealed class CreateCategoryCommandHandler
{
    private readonly IForumRepository _forumRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IForumRepository forumRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumRepository = forumRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var forumOrError = await _forumRepository.GetAsync<ForumCategoryAddable>(request.ForumId, cancellationToken);

        if (forumOrError.TryPickT1(out var error, out var forum)) return error;

        var category = forum.AddCategory(request.Title, request.CreatedBy, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }
}