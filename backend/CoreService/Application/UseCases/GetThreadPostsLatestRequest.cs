using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.UseCases;

public sealed class GetThreadPostsLatestRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds ThreadIds { get; set; }
}

public sealed class GetThreadPostsLatestRequestValidator : AbstractValidator<GetThreadPostsLatestRequest>
{
    public GetThreadPostsLatestRequestValidator()
    {
        RuleForEach(e => e.ThreadIds)
            .GreaterThan(0);
    }
}