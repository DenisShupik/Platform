using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreatePostBookmarkCommandResult = Result<
    Success,
    PostNotFoundError,
    PermissionDeniedError,
    DuplicatePostBookmarkError
>;

[Include(typeof(PostBookmark), PropertyGenerationMode.AsRequired, nameof(PostBookmark.UserId), nameof(PostBookmark.PostId))]
public sealed partial class CreatePostBookmarkCommand : ICommand<CreatePostBookmarkCommandResult>
{
    public required UserIdRole CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed class CreatePostBookmarkCommandHandler :
    ICommandHandler<CreatePostBookmarkCommand, CreatePostBookmarkCommandResult>
{
    private readonly GetPostQueryHandler<PostDto> _getPostQueryHandler;
    private readonly IPostBookmarkReadRepository _postBookmarkReadRepository;
    private readonly IPostBookmarkWriteRepository _postBookmarkWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostBookmarkCommandHandler(
        GetPostQueryHandler<PostDto> getPostQueryHandler,
        IPostBookmarkReadRepository postBookmarkReadRepository,
        IPostBookmarkWriteRepository postBookmarkWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _getPostQueryHandler = getPostQueryHandler;
        _postBookmarkReadRepository = postBookmarkReadRepository;
        _postBookmarkWriteRepository = postBookmarkWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePostBookmarkCommandResult> HandleAsync(
        CreatePostBookmarkCommand command,
        CancellationToken cancellationToken
    )
    {
        var postQuery = new GetPostQuery<PostDto>
        {
            PostId = command.PostId,
            QueriedBy = command.CreatedBy
        };

        var postResult = await _getPostQueryHandler.HandleAsync(postQuery, cancellationToken);

        if (!postResult.GetValue(out _))
        {
            return postResult.Match<CreatePostBookmarkCommandResult>(
                _ => throw new InvalidOperationException(),
                error => error,
                error => error
            );
        }

        if (await _postBookmarkReadRepository.ExistsAsync(command.UserId, command.PostId, cancellationToken))
            return new DuplicatePostBookmarkError(command.UserId, command.PostId);

        await _postBookmarkWriteRepository.AddAsync(
            new PostBookmark(command.UserId, command.PostId, command.CreatedAt),
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success.Instance;
    }
}
