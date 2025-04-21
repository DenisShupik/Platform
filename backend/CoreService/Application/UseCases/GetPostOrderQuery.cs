using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class GetPostOrderQuery
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public required PostId PostId { get; init; }
}

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
        var orderOrError = await _repository.GetPostOrderAsync(request.ThreadId, request.PostId, cancellationToken);

        if (orderOrError.TryPickT1(out _, out var order))
            return new PostNotFoundError(request.ThreadId, request.PostId);

        return order;
    }
}