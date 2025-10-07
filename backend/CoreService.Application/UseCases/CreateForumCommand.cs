using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title), nameof(Forum.CreatedBy),
    nameof(Forum.CreatedAt))]
public sealed partial class CreateForumCommand : ICommand<ForumId>
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public required PolicyValue AccessPolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания раздела
    /// </summary>
    public required PolicyValue CategoryCreatePolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания темы
    /// </summary>
    public required PolicyValue ThreadCreatePolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public required PolicyValue PostCreatePolicyValue { get; init; }
}

public sealed class CreateForumCommandHandler : ICommandHandler<CreateForumCommand, ForumId>
{
    private readonly IAccessWriteRepository _accessWriteRepository;
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IAccessWriteRepository accessWriteRepository,
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _accessWriteRepository = accessWriteRepository;
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForumId> HandleAsync(CreateForumCommand command, CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var accessPolicy = new Policy(PolicyType.Access, command.AccessPolicyValue);
        var categoryCreatePolicy = new Policy(PolicyType.CategoryCreate, command.CategoryCreatePolicyValue);
        var threadCreatePolicy = new Policy(PolicyType.ThreadCreate, command.ThreadCreatePolicyValue);
        var postCreatePolicy = new Policy(PolicyType.PostCreate, command.PostCreatePolicyValue);
        if (command.AccessPolicyValue == PolicyValue.Granted)
        {
            var accessGrant = new Grant(command.CreatedBy, accessPolicy.PolicyId, command.CreatedBy, command.CreatedAt);
            await _accessWriteRepository.AddAsync(accessGrant, cancellationToken);
        }
        await _accessWriteRepository.AddRangeAsync(
            [accessPolicy, categoryCreatePolicy, threadCreatePolicy, postCreatePolicy], cancellationToken);
        var forum = new Forum(command.Title, command.CreatedBy, DateTime.UtcNow, accessPolicy.PolicyId,
            categoryCreatePolicy.PolicyId, threadCreatePolicy.PolicyId, postCreatePolicy.PolicyId);
        await _forumWriteRepository.AddAsync(forum, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return forum.ForumId;
    }
}