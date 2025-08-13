using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.Content),
    nameof(Post.RowVersion))]
public sealed partial class UpdatePostCommand
{
    /// <summary>
    /// Идентификатор пользователя, редактирующего сообщение
    /// </summary>
    public required UserId UpdateBy { get; init; }
}

[GenerateOneOf]
public partial class
    UpdatePostCommandResult : OneOfBase<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>;

public sealed class UpdatePostCommandHandler
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork
    )
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePostCommandResult> HandleAsync(
        UpdatePostCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var postOrError =
            await _postRepository.GetOneAsync(request.ThreadId, request.PostId, cancellationToken);

        if (!postOrError.TryPickT0(out var post, out var error)) return error;

        var postUpdatedOrErrors = post.Update(request.Content, request.RowVersion, request.UpdateBy, DateTime.UtcNow);

        if (!postUpdatedOrErrors.TryPickT0(out _, out var errors))
            return errors.Match<UpdatePostCommandResult>(e => e, e => e);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        return OneOfHelper.Success;
    }
}