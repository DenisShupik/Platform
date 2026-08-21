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

using CreateForumCommandResult = Result<ForumId, PermissionDeniedError>;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title),
    nameof(Forum.CreatedAt))]
public sealed partial class CreateForumCommand : ICreateCommand<CreateForumCommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class CreateForumCommandHandler : ICommandHandler<CreateForumCommand, CreateForumCommandResult>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IForumPolicyEvaluator _policies;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IForumWriteRepository forumWriteRepository,
        IForumPolicyEvaluator policies,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _policies = policies;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateForumCommandResult> HandleAsync(CreateForumCommand command,
        CancellationToken cancellationToken)
    {
        var authorization = await _policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageStructure,
            AuthorizationScope.Platform,
            command.CreatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var forum = new Forum(command.Title, command.RequestedBy.UserId, command.CreatedAt);

        _forumWriteRepository.Add(forum);

        await _unitOfWork.CommitAsync(cancellationToken);

        return forum.ForumId;
    }
}
