using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPostsCountQuery
{
    /// <summary>
    /// Идентификаторы темы
    /// </summary>
    public IdList<ThreadId> ThreadIds { get; set; }
}

public sealed class GetThreadsPostsCountQueryValidator : AbstractValidator<GetThreadsPostsCountQuery>
{
    public GetThreadsPostsCountQueryValidator()
    {
        RuleFor(e => e.ThreadIds)
            .NotEmpty();
    }
}

public sealed class GetThreadsPostsCountQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<ThreadId, long>> HandleAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetThreadsPostsCountAsync(request, cancellationToken);
    }
}