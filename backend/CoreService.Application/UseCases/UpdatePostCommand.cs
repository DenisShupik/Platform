using System.Data;
using CoreService.Application.Authorization;
using Shared.Domain.Abstractions.Results;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using UpdatePostCommandResult = SuccessOr<
    PostNotFoundError,
    PermissionDeniedError,
    PostStaleError,
    ThreadLockedByStateError,
    NonPostAuthorError,
    InsufficientPermissionToEditHeaderPostError,
    InvalidPostContentError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId), nameof(Post.Content),
    nameof(Post.RowVersion), nameof(Post.UpdatedAt))]
public sealed partial class
    UpdatePostCommand : IUpdateCommand<UpdatePostCommandResult>
{
    public required ActorContext RequestedBy { get; init; }
}

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, UpdatePostCommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IForumPolicyEvaluator _policies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostContentProcessor _postContentProcessor;
    private readonly IForumSanctionRepository _sanctions;

    public UpdatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IForumPolicyEvaluator policies,
        IForumSanctionRepository sanctions,
        IUnitOfWork unitOfWork,
        IPostContentProcessor postContentProcessor
    )
    {
        _postWriteRepository = postWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _policies = policies;
        _sanctions = sanctions;
        _unitOfWork = unitOfWork;
        _postContentProcessor = postContentProcessor;
    }

    public async Task<UpdatePostCommandResult> HandleAsync(UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        var processedContentOrError = _postContentProcessor.Process(command.Content);
        if (!processedContentOrError.TryGetValue(out var processedContent, out var contentError))
            return contentError;

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken)).TryGetValue(out var post,
                out var errors1)) return errors1;

        if (!(await _threadWriteRepository.GetOneAsync(post.ThreadId, LockMode.ForShare, cancellationToken)).TryGetValue(out var thread,
                out _)) return new PostNotFoundError();

        if (await _sanctions.IsThreadParticipationRestrictedAsync(
                command.RequestedBy.UserId,
                post.ThreadId,
                command.UpdatedAt,
                cancellationToken))
            return new PermissionDeniedError();

        var categoryResult = await _categoryWriteRepository.GetAsync(thread.CategoryId, cancellationToken);
        if (!categoryResult.TryGetValue(out var category, out _)) return new PostNotFoundError();

        var authorization = await _policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.EditAnyPost,
            AuthorizationScope.Thread(category.ForumId, category.CategoryId, thread.ThreadId),
            command.UpdatedAt,
            cancellationToken);
        var canEditAnyPost = !authorization.TryGetFailure(out _);

        var updateResult = thread.UpdatePost(
            post,
            processedContent.Content,
            command.RowVersion,
            command.RequestedBy.UserId,
            command.UpdatedAt,
            canEditAnyPost);
        if (updateResult.TryGetFailure(out var updateFailure)) return updateFailure;

        _postWriteRepository.SetSearchText(post, processedContent.SearchText);

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

        return SuccessOr.Success;
    }
}
