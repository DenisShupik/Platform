using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using OneOf;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class GetPostIndexQuery;

[GenerateOneOf]
public partial class GetPostIndexQueryResult : OneOfBase<PostIndex, PostNotFoundError>;

public sealed class GetPostIndexQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetPostIndexQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetPostIndexQueryResult> HandleAsync(GetPostIndexQuery request,
        CancellationToken cancellationToken)
    {
        var orderOrError = await _repository.GetPostIndexAsync(request.PostId, cancellationToken);

        if (!orderOrError.TryPickT0(out var order, out var error)) return error;

        return order;
    }
}