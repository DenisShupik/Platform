using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;

namespace CoreService.Application.UseCases;

using CommandResult = Result<
    PostId,
    ThreadNotFoundError,
    ThreadLockedByStateError,
    NonThreadOwnerError,
    PostLimitReachedError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy), nameof(Post.CreatedAt))]
public sealed partial class CreatePostCommand : ICreateCommand<CommandResult>
{
    public required Role CreatorRole { get; init; }
}

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CommandResult>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
        _postWriteRepository = postWriteRepository;
    }

    public async Task<CommandResult> HandleAsync(CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetOneAsync(command.ThreadId, LockMode.ForUpdate, cancellationToken);

        if (!threadOrError.ValueOrErrors(out var thread, out var errors1)) return errors1;
        
        if (!thread.AddPost(command.Content, command.CreatedBy, DateTime.UtcNow).ValueOrErrors(out var post, out var errors2)) return errors2.Value;

        _postWriteRepository.Add(post);

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