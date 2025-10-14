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
    ReadPolicyRestrictedError,
    CategoryCreatePolicyRestrictedError,
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
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IAccessWriteRepository _accessWriteRepository;
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        IAccessWriteRepository accessWriteRepository,
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
        _accessWriteRepository = accessWriteRepository;
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
    }

    public async Task<CreateCategoryCommandResult> HandleAsync(CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserCanCreateCategoryAsync(command.CreatedBy, command.ForumId,
                timestamp, cancellationToken);

        if (!accessCheckResult.TryOrExtend<CategoryId, PolicyDowngradeError>(out var accessRestrictedError)) return accessRestrictedError.Value;

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