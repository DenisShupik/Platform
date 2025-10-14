using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CreateForumCommandResult = Result<ForumId, PolicyDowngradeError>;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title), nameof(Forum.CreatedBy),
    nameof(Forum.CreatedAt))]
public sealed partial class CreateForumCommand : ICommand<CreateForumCommandResult>
{
    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public required PolicyValue? ReadPolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания раздела
    /// </summary>
    public required PolicyValue? CategoryCreatePolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания темы
    /// </summary>
    public required PolicyValue? ThreadCreatePolicyValue { get; init; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public required PolicyValue? PostCreatePolicyValue { get; init; }
}

public sealed class CreateForumCommandHandler : ICommandHandler<CreateForumCommand, CreateForumCommandResult>
{
    private readonly IAccessWriteRepository _accessWriteRepository;
    private readonly IPortalWriteRepository _portalWriteRepository;
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IAccessWriteRepository accessWriteRepository,
        IPortalWriteRepository portalWriteRepository,
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _accessWriteRepository = accessWriteRepository;
        _portalWriteRepository = portalWriteRepository;
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateForumCommandResult> HandleAsync(CreateForumCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var portal = await _portalWriteRepository.GetAsync(cancellationToken);

        if (!portal
                .AddForum(command.Title, command.CreatedBy, DateTime.UtcNow, command.ReadPolicyValue,
                    command.CategoryCreatePolicyValue, command.ThreadCreatePolicyValue, command.PostCreatePolicyValue)
                .TryGet(out var forum, out var error)
           )
            return error;

        await _forumWriteRepository.AddAsync(forum, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return forum.ForumId;
    }
}