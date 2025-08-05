using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Generator.Attributes;
using OneOf;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.PostId))]
public sealed partial class GetPostQuery;

[GenerateOneOf]
public partial class GetPostQueryResult<T> : OneOfBase<T, ThreadNotFoundError, PostNotFoundError>;

public sealed class GetPostQueryHandler
{
    private readonly IPostReadRepository _repository;

    public GetPostQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    private async Task<GetPostQueryResult<T>> HandleAsync<T>(GetPostQuery request,
        CancellationToken cancellationToken)
    {
        // TODO: Исследовать возможность избежать лишней аллокации
        return new GetPostQueryResult<T>(await _repository.GetOneAsync<T>(request.ThreadId, request.PostId,
            cancellationToken));
    }

    public Task<GetPostQueryResult<PostDto>> HandleAsync(GetPostQuery request,
        CancellationToken cancellationToken)
    {
        return HandleAsync<PostDto>(request, cancellationToken);
    }
}