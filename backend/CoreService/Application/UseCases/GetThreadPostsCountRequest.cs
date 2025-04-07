using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.UseCases;

public sealed class GetThreadPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds ThreadIds { get; set; }
}

public sealed class GetThreadPostsCountRequestValidator : AbstractValidator<GetThreadPostsCountRequest>
{
    public GetThreadPostsCountRequestValidator()
    {
        RuleForEach(e => e.ThreadIds)
            .GreaterThan(0);
    }
}