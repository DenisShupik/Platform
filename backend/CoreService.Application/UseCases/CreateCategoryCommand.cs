using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreateCategoryCommandResult = Result<
    CategoryId,
    ForumNotFoundError,
    PolicyViolationError,
    PolicyRestrictedError,
    PolicyDowngradeError
>;

[Omit(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId), nameof(Category.Threads),
    nameof(Category.ReadPolicyId), nameof(Category.ThreadCreatePolicyId), nameof(Category.PostCreatePolicyId))]
public sealed partial class CreateCategoryCommand : ICommand<CreateCategoryCommandResult>
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public required PolicyValue? ReadPolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания темы
    /// </summary>
    public required PolicyValue? ThreadCreatePolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public required PolicyValue? PostCreatePolicyValue { get; init; }
}

public sealed class
    CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResult>
{
    private readonly IAccessReadRepository _accessReadRepository;
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IAccessReadRepository accessReadRepository,
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
        _accessReadRepository = accessReadRepository;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        {
            if (!(await _accessReadRepository.EvaluatedForumPolicy(command.ForumId, command.CreatedBy,
                    PolicyType.CategoryCreate, command.CreatedAt, cancellationToken))
                .TryOrExtend<CategoryId, PolicyDowngradeError>(out var errors)
               ) return errors.Value;
        }

        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var forumOrError =
                await _forumWriteRepository.GetForumCategoryAddableAsync(command.ForumId, cancellationToken);

            if (!forumOrError.TryGet(out var forum, out var error)) return error;

            if (!forum
                    .AddCategory(command.Title, command.CreatedBy, command.CreatedAt, command.ReadPolicyValue,
                        command.ThreadCreatePolicyValue, command.PostCreatePolicyValue)
                    .TryGet(out var category, out var errors)
               )
                return errors;

            await _unitOfWork.CommitAsync(cancellationToken);

            return category.CategoryId;
        }
    }
}