using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class GetThreadQuery
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId ThreadId { get; init; }
}

public sealed class GetThreadQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private Task<OneOf<T, ThreadNotFoundError>> HandleAsync<T>(
        GetThreadQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetByIdAsync<T>(request.ThreadId, cancellationToken);
    }

    public Task<OneOf<ThreadDto, ThreadNotFoundError>> HandleAsync(
        GetThreadQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<ThreadDto>(request, cancellationToken);
    }
}