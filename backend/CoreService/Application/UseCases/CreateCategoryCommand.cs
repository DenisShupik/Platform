using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class CreateCategoryCommand
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public required ForumId ForumId { get; init; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public required CategoryTitle Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId UserId { get; init; }
}

[GenerateOneOf]
public partial class CreateCategoryCommandResult : OneOfBase<ForumNotFoundError, CategoryId>;

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

        var category = forum.AddCategory(request.Title, request.UserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }
}