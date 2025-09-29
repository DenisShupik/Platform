using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

using CreatePostCommandResult = Result<
    PostId,
    ThreadNotFoundError,
    AccessLevelError,
    AccessRestrictedError,
    NonThreadOwnerError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy))]
public sealed partial class CreatePostCommand : ICommand<CreatePostCommandResult>;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostCommandResult>
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

    public async Task<CreatePostCommandResult> HandleAsync(CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserWriteAccessAsync(command.CreatedBy, command.ThreadId,
                cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<PostId, NonThreadOwnerError>(out _, out var accessRestrictedError))
            return accessRestrictedError.Value;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetThreadPostAddableAsync(command.ThreadId, cancellationToken);

        if (!threadOrError.TryPick(out var thread, out var threadError)) return threadError;

        var postOrError = thread.AddPost(command.Content, command.CreatedBy, DateTime.UtcNow);

        if (!postOrError.TryPick(out var post, out var postError)) return postError;

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