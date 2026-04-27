using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Enums;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    Success,
    PostNotFoundError,
    ThreadNotFoundError,
    ThreadLockedByStateError,
    NonThreadOwnerError,
    ApprovedHeaderPostDeletionForbiddenError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class DeletePostCommand : IDeleteCommand<CommandResult>
{
    public UserId DeletedBy { get; init; }
    public DateTime DeletedAt { get; init; }
    public Role DeleterRole { get; init; }
}

public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand, CommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _postWriteRepository = postWriteRepository;
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> HandleAsync(DeletePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (!(await _postWriteRepository.GetOneAsync(command.PostId, cancellationToken)).ValueOrErrors(out var post,
                out var errors1)) return errors1;

        if (!(await _threadWriteRepository.GetOneAsync(post.ThreadId, LockMode.ForUpdate, cancellationToken)).ValueOrErrors(out var thread,
                out var errors2)) return errors2;

        if (!thread.DeletePost(post, command.DeletedBy).SuccessOrErrors(out var errors3)) return errors3.Value;
        
        _postWriteRepository.Remove(post);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Success.Instance;
    }
}