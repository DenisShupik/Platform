using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using Shared.Application.Interfaces;
using OneOf;
using OneOf.Types;
using Shared.TypeGenerator.Attributes;
using Shared.Domain.Helpers;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId), nameof(Post.Content),
    nameof(Post.RowVersion))]
public sealed partial class
    UpdatePostCommand : ICommand<OneOf<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>>
{
    /// <summary>
    /// Идентификатор пользователя, редактирующего сообщение
    /// </summary>
    public required UserId UpdateBy { get; init; }
}

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand,
    OneOf<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>>
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

    public async Task<OneOf<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>> HandleAsync(
        UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var postOrError = await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken);

        if (!postOrError.TryPickT0(out var post, out var error)) return error;

        var postUpdatedOrErrors = post.Update(command.Content, command.RowVersion, command.UpdateBy, DateTime.UtcNow);

        if (!postUpdatedOrErrors.TryPickT0(out _, out var errors))
            return errors.Match<OneOf<Success, PostNotFoundError, NonPostAuthorError, PostStaleError>>(e => e, e => e);

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