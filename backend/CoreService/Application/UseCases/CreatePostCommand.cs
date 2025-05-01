using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using OneOf;
using SharedKernel.Application.Interfaces;

namespace CoreService.Application.UseCases;

[IncludeAsRequired(typeof(Post),nameof(Post.ThreadId), nameof(Post.Content), nameof(Post.CreatedBy))]
public sealed partial class CreatePostCommand;

[GenerateOneOf]
public partial class CreatePostCommandResult : OneOfBase<ThreadNotFoundError, NonThreadOwnerError, PostId>;

public sealed class CreatePostCommandHandler
{
    private readonly IThreadRepository _threadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IThreadRepository threadRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadRepository = threadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePostCommandResult> HandleAsync(CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadRepository.GetWithLockAsync<ThreadPostAddable>(request.ThreadId, cancellationToken);

        if (threadOrError.TryPickT1(out var error, out var thread)) return error;

        var postOrError = thread.AddPost(request.Content, request.CreatedBy, DateTime.UtcNow);

        if (postOrError.TryPickT0(out var postError, out var post)) return postError;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return post.PostId;
    }
}