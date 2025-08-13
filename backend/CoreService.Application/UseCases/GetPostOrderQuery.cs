using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Generator.Attributes;
using OneOf;

namespace CoreService.Application.UseCases;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class GetPostOrderQuery;

[GenerateOneOf]
public partial class GetPostOrderQueryResult : OneOfBase<long, PostNotFoundError>;

public sealed class GetPostOrderQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetPostOrderQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetPostOrderQueryResult> HandleAsync(GetPostOrderQuery request,
        CancellationToken cancellationToken)
    {
        var orderOrError = await _repository.GetPostOrderAsync(request.PostId, cancellationToken);

        if (!orderOrError.TryPickT0(out var order, out var error)) return error;

        return order;
    }
}