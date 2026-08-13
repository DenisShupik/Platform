using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

using DeletePostBookmarkCommandResult = SuccessOr<PostBookmarkNotFoundError>;

[Include(typeof(PostBookmark), PropertyGenerationMode.AsRequired, nameof(PostBookmark.UserId), nameof(PostBookmark.PostId))]
public sealed partial class DeletePostBookmarkCommand : ICommand<DeletePostBookmarkCommandResult>;

public sealed class DeletePostBookmarkCommandHandler :
    ICommandHandler<DeletePostBookmarkCommand, DeletePostBookmarkCommandResult>
{
    private readonly IPostBookmarkWriteRepository _postBookmarkWriteRepository;

    public DeletePostBookmarkCommandHandler(IPostBookmarkWriteRepository postBookmarkWriteRepository)
    {
        _postBookmarkWriteRepository = postBookmarkWriteRepository;
    }

    public Task<DeletePostBookmarkCommandResult> HandleAsync(
        DeletePostBookmarkCommand command,
        CancellationToken cancellationToken
    )
    {
        return _postBookmarkWriteRepository.ExecuteRemoveAsync(
            command.UserId,
            command.PostId,
            cancellationToken
        );
    }
}
