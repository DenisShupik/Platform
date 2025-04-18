using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using FluentValidation;
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
    public required string Content { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId UserId { get; init; }
}

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(e => e.Content)
            .NotEmpty()
            .MaximumLength(Post.ContentMaxLength);
    }
}

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

    public async Task<OneOf<PostId, ThreadNotFoundError>> HandleAsync(CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var projectionOrError =
            await _threadRepository.GetWithLockAsync<ThreadPostAddable>(request.ThreadId, cancellationToken);

        if (projectionOrError.TryPickT1(out var error, out var projection)) return error;

        var post = new Post
        {
            ThreadId = request.ThreadId,
            Content = request.Content,
            Created = DateTime.UtcNow,
            CreatedBy = request.UserId
        };
        projection.AddPost(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return post.PostId;
    }
}