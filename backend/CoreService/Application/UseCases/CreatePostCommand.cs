using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class CreatePostCommand
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public required PostContent Content { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId UserId { get; init; }
}

[GenerateOneOf]
public partial class CreatePostCommandResult : OneOfBase<PostId, ThreadNotFoundError, NonThreadOwnerError>;

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

        var post = new Post
        {
            ThreadId = request.ThreadId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.UserId
        };

        var maybeError = thread.AddPost(post);

        if (maybeError != null) return maybeError;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return post.PostId;
    }
}