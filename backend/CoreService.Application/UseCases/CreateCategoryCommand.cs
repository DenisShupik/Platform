using System.Data;
using CoreService.Application.Authorization;
using Shared.Domain.Abstractions.Results;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreateCategoryCommandResult = Result<
    CategoryId,
    PermissionDeniedError,
    ForumNotFoundError
>;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.ForumId), nameof(Category.Title),
    nameof(Category.CreatedAt))]
public sealed partial class CreateCategoryCommand : ICreateCommand<CreateCategoryCommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class
    CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IForumPolicyEvaluator _policies;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IForumWriteRepository forumWriteRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IForumPolicyEvaluator policies,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _policies = policies;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var authorization = await _policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageStructure,
            AuthorizationScope.Forum(command.ForumId),
            command.CreatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var forumOrError =
            await _forumWriteRepository.GetAsync<Forum>(command.ForumId, cancellationToken);

        if (!forumOrError.TryGetValue(out var forum, out var error)) return error;

        var category = forum.AddCategory(command.Title, command.RequestedBy.UserId, command.CreatedAt);

        _categoryWriteRepository.Add(category);

        await _unitOfWork.CommitAsync(cancellationToken);

        return category.CategoryId;
    }
}
