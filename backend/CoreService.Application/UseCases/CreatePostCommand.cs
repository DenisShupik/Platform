using System.Data;
using CoreService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    PostId,
    PermissionDeniedError,
    ThreadNotFoundError,
    ThreadLockedByStateError,
    NonThreadOwnerError,
    PostLimitReachedError,
    InvalidPostContentError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedAt))]
public sealed partial class CreatePostCommand : ICreateCommand<CommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostContentProcessor _postContentProcessor;
    private readonly IForumSanctionRepository _sanctions;

    public CreatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork,
        IPostContentProcessor postContentProcessor,
        IForumSanctionRepository sanctions
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
        _postWriteRepository = postWriteRepository;
        _postContentProcessor = postContentProcessor;
        _sanctions = sanctions;
    }

    public async Task<CommandResult> HandleAsync(CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var processedContentOrError = _postContentProcessor.Process(command.Content);

        if (!processedContentOrError.TryGetValue(out var processedContent, out var contentError))
            return contentError;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken);
        if (!threadOrError.TryGetValue(out var thread, out var errors1)) return errors1;

        if (await _sanctions.IsThreadParticipationRestrictedAsync(
                command.RequestedBy.UserId,
                command.ThreadId,
                command.CreatedAt,
                cancellationToken))
            return new PermissionDeniedError();

        var postOrError = thread.AddPost(processedContent.Content, command.RequestedBy.UserId, command.CreatedAt);
        if (!postOrError.TryGetValue(out var post, out var postFailure)) return postFailure;

        _postWriteRepository.Add(post, processedContent.SearchText);

        await _unitOfWork.PublishEventAsync(
            new PostAddedEvent
            {
                ThreadId = post.ThreadId,
                PostId = post.PostId,
                CreatedBy = post.CreatedBy,
                CreatedAt = post.CreatedAt
            },
            cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return post.PostId;
    }
}
