using System.Data;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.UseCases;

using CreatePostCommandResult = Result<
    PostId,
    ThreadNotFoundError,
    PolicyViolationError,
    PolicyRestrictedError,
    NonThreadOwnerError
>;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.Content),
    nameof(Post.CreatedBy), nameof(Post.CreatedAt))]
public sealed partial class CreatePostCommand : ICommand<CreatePostCommandResult>;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostCommandResult>
{
    private readonly IAccessReadRepository _accessReadRepository;
    private readonly IThreadWriteRepository _threadWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IAccessReadRepository accessReadRepository,
        IThreadWriteRepository threadWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _accessReadRepository = accessReadRepository;
        _threadWriteRepository = threadWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePostCommandResult> HandleAsync(CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        {
            if (!(await _accessReadRepository.EvaluatedThreadPolicy(command.ThreadId, command.CreatedBy,
                    PolicyType.PostCreate, command.CreatedAt, cancellationToken))
                .TryOrExtend<PostId, NonThreadOwnerError>(out var error))
                return error.Value;
        }

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var threadOrError =
            await _threadWriteRepository.GetThreadPostAddableAsync(command.ThreadId, cancellationToken);

        if (!threadOrError.TryGet(out var thread, out var threadError)) return threadError;

        var postOrError = thread.AddPost(command.Content, command.CreatedBy, DateTime.UtcNow);

        if (!postOrError.TryGet(out var post, out var postError)) return postError;

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