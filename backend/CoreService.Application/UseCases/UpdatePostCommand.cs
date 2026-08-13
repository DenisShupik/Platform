using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using UpdatePostCommandResult = SuccessOr<
    PostNotFoundError,
    ThreadNotFoundError,
    PostStaleError,
    ThreadLockedByStateError,
    NonPostAuthorError,
    InsufficientRoleToEditHeaderPostError,
    InvalidPostContentError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId), nameof(Post.Content),
    nameof(Post.RowVersion), nameof(Post.UpdatedBy), nameof(Post.UpdatedAt))]
public sealed partial class
    UpdatePostCommand : IUpdateCommand<UpdatePostCommandResult>
{
    public required Role UpdaterRole { get; init; }
}

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, UpdatePostCommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostContentProcessor _postContentProcessor;

    public UpdatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork,
        IPostContentProcessor postContentProcessor
    )
    {
        _postWriteRepository = postWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
        _postContentProcessor = postContentProcessor;
    }

    public async Task<UpdatePostCommandResult> HandleAsync(UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        var processedContentOrError = _postContentProcessor.Process(command.Content);
        if (!processedContentOrError.TryGetValue(out var processedContent, out var contentError))
            return contentError;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken)).TryGetValue(out var post,
                out var errors1)) return errors1;

        if (!(await _threadWriteRepository.GetOneAsync(post.ThreadId, LockMode.ForShare, cancellationToken)).TryGetValue(out var thread,
                out var errors2)) return errors2;

        var updateResult = thread.UpdatePost(
            post,
            processedContent.Content,
            command.RowVersion,
            command.UpdatedBy,
            command.UpdatedAt,
            command.UpdaterRole);
        if (updateResult.TryGetFailure(out var updateFailure)) return updateFailure;

        _postWriteRepository.SetSearchText(post, processedContent.SearchText);

        await _unitOfWork.PublishEventAsync(
            new PostUpdatedEvent
            {
                ThreadId = post.ThreadId,
                PostId = post.PostId,
                UpdatedBy = post.UpdatedBy,
                UpdatedAt = post.UpdatedAt
            },
            cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return SuccessOr.Success;
    }
}
