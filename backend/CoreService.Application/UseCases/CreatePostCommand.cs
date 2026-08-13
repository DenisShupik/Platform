using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.TypeGenerator.Attributes;

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
        var processedContentOrError = _postContentProcessor.Process(command.Content);

        if (!processedContentOrError.TryGetValue(out var processedContent, out var contentError))
            return contentError;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken);
        if (!threadOrError.TryGetValue(out var thread, out var errors1)) return errors1;

        var postOrError = thread.AddPost(processedContent.Content, command.CreatedBy, DateTime.UtcNow);
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
