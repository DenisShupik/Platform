using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using OneOf;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy))]
public sealed partial class
    CreatePostCommand : ICommand<OneOf<PostId, ThreadNotFoundError, ThreadAccessRestrictedError, NonThreadOwnerError>>;

public sealed class
    CreatePostCommandHandler : ICommandHandler<CreatePostCommand,
    OneOf<PostId, ThreadNotFoundError, ThreadAccessRestrictedError, NonThreadOwnerError>>
{
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork,
        IAccessRestrictionReadRepository accessRestrictionReadRepository
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
    }

    public async Task<OneOf<PostId, ThreadNotFoundError, ThreadAccessRestrictedError, NonThreadOwnerError>> HandleAsync(
        CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetThreadPostAddableAsync(command.ThreadId, cancellationToken);

        if (!threadOrError.TryPickT0(out var thread, out var threadError)) return threadError;

        var successOrError =
            await _accessRestrictionReadRepository.CanUserPostInThreadAsync(command.CreatedBy, command.ThreadId,
                cancellationToken);

        if (!successOrError.TryPickT0(out _, out var accessRestrictedError)) return accessRestrictedError;

        var postOrError = thread.AddPost(command.Content, command.CreatedBy, DateTime.UtcNow);

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