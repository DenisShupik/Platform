using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Paging;

namespace TopicService.Application.DTOs;

public sealed class GetPostsRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public LongIds? TopicIds { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public bool TopicLatest { get; set; } = false;
}

public sealed class GetPostsRequestValidator : LongKeysetPageRequestValidator<GetPostsRequest>
{
    public GetPostsRequestValidator()
    {
        RuleForEach(e => e.TopicIds)
            .GreaterThan(0)
            .When(e => e.TopicIds != null);
    }
}