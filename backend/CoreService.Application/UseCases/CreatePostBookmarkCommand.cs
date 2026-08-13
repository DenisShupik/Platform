using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using CreatePostBookmarkCommandResult = SuccessOr<
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
    private readonly IPostBookmarkWriteRepository _postBookmarkWriteRepository;

    public CreatePostBookmarkCommandHandler(
        GetPostQueryHandler<PostDto> getPostQueryHandler,
        IPostBookmarkWriteRepository postBookmarkWriteRepository
    )
    {
        _getPostQueryHandler = getPostQueryHandler;
        _postBookmarkWriteRepository = postBookmarkWriteRepository;
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

        if (!postResult.TryGetValue(out _, out var postFailure)) return postFailure;

        var addResult = await _postBookmarkWriteRepository.ExecuteAddAsync(
            new PostBookmark(command.UserId, command.PostId, command.CreatedAt),
            cancellationToken);

        if (addResult.TryGetFailure(out var addFailure)) return addFailure;

        return SuccessOr.Success;
    }
}
