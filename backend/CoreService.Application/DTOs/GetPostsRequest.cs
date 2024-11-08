using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Paging;

namespace CoreService.Application.DTOs;

public sealed class GetPostsRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public LongIds? ThreadIds { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public bool ThreadLatest { get; set; } = false;
}

public sealed class GetPostsRequestValidator : LongKeysetPageRequestValidator<GetPostsRequest>
{
    public GetPostsRequestValidator()
    {
        RuleForEach(e => e.ThreadIds)
            .GreaterThan(0)
            .When(e => e.ThreadIds != null);
    }
}