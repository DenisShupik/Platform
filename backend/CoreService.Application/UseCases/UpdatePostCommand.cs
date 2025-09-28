using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId), nameof(Post.Content),
    nameof(Post.RowVersion))]
public sealed partial class
    UpdatePostCommand : ICommand<Result<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>>
{
    /// <summary>
    /// Идентификатор пользователя, редактирующего сообщение
    /// </summary>
    public required UserId UpdateBy { get; init; }
}

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand,
    Result<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _postWriteRepository = postWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>> HandleAsync(
        UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var postOrError = await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken);

        if (!postOrError.TryPick(out var post, out var error)) return error;

        var postUpdatedOrErrors = post.Update(command.Content, command.RowVersion, command.UpdateBy, DateTime.UtcNow);

        if (!postUpdatedOrErrors.TryPickOrExtend<PostNotFoundError>(out _, out var errors))
            return errors.Value;

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

        return Success.Instance;
    }
}