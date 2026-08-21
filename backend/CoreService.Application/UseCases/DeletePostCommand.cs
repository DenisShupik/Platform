using System.Data;
using CoreService.Application.Authorization;
using Shared.Domain.Abstractions.Results;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    PostNotFoundError,
    ThreadNotFoundError,
    ThreadLockedByStateError,
    NonPostAuthorError,
    ApprovedHeaderPostDeletionForbiddenError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class DeletePostCommand : IDeleteCommand<CommandResult>
{
    public required ActorContext RequestedBy { get; init; }
    public required DateTime DeletedAt { get; init; }
}

public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand, CommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IForumPolicyEvaluator _policies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForumSanctionRepository _sanctions;

    public DeletePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        ICategoryWriteRepository categoryWriteRepository,
        IForumPolicyEvaluator policies,
        IForumSanctionRepository sanctions,
        IUnitOfWork unitOfWork
    )
    {
        _postWriteRepository = postWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _policies = policies;
        _sanctions = sanctions;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(DeletePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken)).TryGetValue(out var post,
                out var errors1)) return errors1;

        if (!(await _threadWriteRepository.GetOneAsync(post.ThreadId, LockMode.ForUpdate, cancellationToken)).TryGetValue(out var thread,
                out var errors2)) return errors2;

        if (await _sanctions.IsThreadParticipationRestrictedAsync(
                command.RequestedBy.UserId,
                post.ThreadId,
                command.DeletedAt,
                cancellationToken))
            return new PermissionDeniedError();

        var categoryResult = await _categoryWriteRepository.GetAsync(thread.CategoryId, cancellationToken);
        if (!categoryResult.TryGetValue(out var category, out _)) return new ThreadNotFoundError();

        var authorization = await _policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.DeleteAnyPost,
            AuthorizationScope.Thread(category.ForumId, category.CategoryId, thread.ThreadId),
            command.DeletedAt,
            cancellationToken);
        var canDeleteAnyPost = !authorization.TryGetFailure(out _);

        if (thread.DeletePost(post, command.RequestedBy.UserId, canDeleteAnyPost).TryGetFailure(out var failure))
            return failure;

        _postWriteRepository.Remove(post);

        await _unitOfWork.CommitAsync(cancellationToken);

        return SuccessOr.Success;
    }
}
