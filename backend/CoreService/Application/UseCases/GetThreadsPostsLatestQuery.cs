using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPostsLatestQuery
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public IdList<ThreadId> ThreadIds { get; set; }
}

public sealed class GetThreadsPostsLatestQueryValidator : AbstractValidator<GetThreadsPostsLatestQuery>
{
    public GetThreadsPostsLatestQueryValidator()
    {
        RuleFor(e => e.ThreadIds)
            .NotEmpty();
    }
}

public sealed class GetThreadsPostsLatestQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsLatestQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<ThreadId, T>> HandleAsync<T>(
        GetThreadsPostsLatestQuery request, CancellationToken cancellationToken
    )
        where T : IHasThreadId
    {
        return _repository.GetThreadsPostsLatestAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<ThreadId, PostDto>> HandleAsync(
        GetThreadsPostsLatestQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<PostDto>(request, cancellationToken);
    }
}