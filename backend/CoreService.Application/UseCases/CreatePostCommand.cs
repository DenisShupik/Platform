using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using OneOf;
using SharedKernel.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy))]
public sealed partial class CreatePostCommand;

[GenerateOneOf]
public partial class CreatePostCommandResult : OneOfBase<PostId, ThreadNotFoundError, NonThreadOwnerError>;

public sealed class CreatePostCommandHandler
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePostCommandResult> HandleAsync(CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetThreadPostAddableAsync(request.ThreadId, cancellationToken);

        if (!threadOrError.TryPickT0(out var thread, out var threadError)) return threadError;

        var postOrError = thread.AddPost(request.Content, request.CreatedBy, DateTime.UtcNow);

        if (!postOrError.TryPickT0(out var post, out var postError)) return postError;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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