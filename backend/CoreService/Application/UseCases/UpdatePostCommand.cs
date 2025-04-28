using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;

namespace CoreService.Application.UseCases;

public sealed class UpdatePostCommand
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public required PostId PostId { get; init; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public required PostContent Content { get; init; }

    /// <summary>
    /// Идентификатор пользователя, редактирующего сообщение
    /// </summary>
    public required UserId UpdateBy { get; init; }

    /// <summary>
    /// Маркер версии записи
    /// </summary>
    public required uint RowVersion { get; init; }
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