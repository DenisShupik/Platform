using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;

namespace CoreService.Application.UseCases;

using CreateForumCommandResult = Result<ForumId, PermissionDeniedError>;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title), nameof(Forum.CreatedBy),
    nameof(Forum.CreatedAt))]
public sealed partial class CreateForumCommand : ICreateCommand<CreateForumCommandResult>
{
    public required Role CreatorRole { get; init; }
}

public sealed class CreateForumCommandHandler : ICommandHandler<CreateForumCommand, CreateForumCommandResult>
{
    private readonly IForumWriteRepository _forumWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateForumCommandHandler(
        IForumWriteRepository forumWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _forumWriteRepository = forumWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateForumCommandResult> HandleAsync(CreateForumCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CreatorRole < Role.Moderator) return new PermissionDeniedError();

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var forum = new Forum(command.Title, command.CreatedBy, DateTime.UtcNow);

        _forumWriteRepository.Add(forum);

        await _unitOfWork.CommitAsync(cancellationToken);

        return forum.ForumId;
    }
}