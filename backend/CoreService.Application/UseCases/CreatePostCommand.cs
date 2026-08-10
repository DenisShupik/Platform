using System.Data;
using System.Diagnostics;
using CoreService.Application.Diagnostics;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    PostId,
    ThreadNotFoundError,
    ThreadLockedByStateError,
    NonThreadOwnerError,
    PostLimitReachedError,
    InvalidPostContentError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy), nameof(Post.CreatedAt))]
public sealed partial class CreatePostCommand : ICreateCommand<CommandResult>
{
    public required Role CreatorRole { get; init; }
}

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostContentProcessor _postContentProcessor;

    public CreatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork,
        IPostContentProcessor postContentProcessor
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
        _postWriteRepository = postWriteRepository;
        _postContentProcessor = postContentProcessor;
    }

    public async Task<CommandResult> HandleAsync(CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        Result<ProcessedPostContent, InvalidPostContentError> processedContentOrError;
        using (CoreServiceActivitySource.StartCreatePostActivity(
                   CoreServiceActivitySource.PreparePostContent,
                   command.ThreadId))
        {
            processedContentOrError = _postContentProcessor.Process(command.Content);
        }

        if (!processedContentOrError.ValueOrErrors(out var processedContent, out var contentError))
            return contentError;

        Activity? lockActivity = null;
        try
        {
            IDbContextTransaction transaction;
            using (CoreServiceActivitySource.StartCreatePostActivity(
                       CoreServiceActivitySource.BeginPostTransaction,
                       command.ThreadId))
            {
                transaction =
                    await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            }

            await using (transaction)
            {
                Result<Thread, ThreadNotFoundError> threadOrError;
                using (CoreServiceActivitySource.StartCreatePostActivity(
                           CoreServiceActivitySource.LoadThreadForPost,
                           command.ThreadId))
                {
                    threadOrError =
                        await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate,
                            cancellationToken);
                }

                if (!threadOrError.ValueOrErrors(out var thread, out var errors1)) return errors1;

                lockActivity =
                    CoreServiceActivitySource.StartCreatePostActivity(
                        CoreServiceActivitySource.HoldThreadLockForPost,
                        command.ThreadId);

                Post? post;
                using (CoreServiceActivitySource.StartCreatePostActivity(
                           CoreServiceActivitySource.AddPostToThread,
                           command.ThreadId))
                {
                    var postOrError = thread.AddPost(processedContent.Content, command.CreatedBy, DateTime.UtcNow);
                    if (!postOrError.ValueOrErrors(out post, out _))
                        return postOrError.Match<CommandResult>(
                            _ => throw new InvalidOperationException(
                                "Successful post creation cannot be mapped to an error."),
                            error => error,
                            error => error,
                            error => error);
                }

                _postWriteRepository.Add(post, processedContent.SearchText);

                using (CoreServiceActivitySource.StartCreatePostActivity(
                           CoreServiceActivitySource.PublishPostAdded,
                           command.ThreadId))
                {
                    await _unitOfWork.PublishEventAsync(
                        new PostAddedEvent
                        {
                            ThreadId = post.ThreadId,
                            PostId = post.PostId,
                            CreatedBy = post.CreatedBy,
                            CreatedAt = post.CreatedAt
                        },
                        cancellationToken);
                }

                using (CoreServiceActivitySource.StartCreatePostActivity(
                           CoreServiceActivitySource.CommitPost,
                           command.ThreadId))
                {
                    await _unitOfWork.CommitAsync(cancellationToken);
                }

                return post.PostId;
            }
        }
        finally
        {
            lockActivity?.Dispose();
        }
    }
}
