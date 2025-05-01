using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Generator.Attributes;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;

namespace CoreService.Application.UseCases;

[IncludeAsRequired(typeof(Post),nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.Content), nameof(Post.RowVersion))]
public sealed partial class UpdatePostCommand
{
    /// <summary>
    /// Идентификатор пользователя, редактирующего сообщение
    /// </summary>
    public required UserId UpdateBy { get; init; }
}

[GenerateOneOf]
public partial class
    UpdatePostCommandResult : OneOfBase<PostNotFoundError, NonPostAuthorError, PostStaleError, Success>;

public sealed class UpdatePostCommandHandler
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork
    )
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePostCommandResult> HandleAsync(
        UpdatePostCommand request,
        CancellationToken cancellationToken)
    {
        var postOrError =
            await _postRepository.GetOneAsync(request.ThreadId, request.PostId, cancellationToken);

        if (postOrError.TryPickT0(out var error, out var post)) return error;

        var result = post.Update(request.Content, request.RowVersion, request.UpdateBy, DateTime.UtcNow);

        return await result.Match(
            x => Task.FromResult<UpdatePostCommandResult>(x),
            x => Task.FromResult<UpdatePostCommandResult>(x),
            async _ =>
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return (UpdatePostCommandResult)OneOfHelper.Success;
            }
        );
    }
}