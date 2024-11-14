using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Paging;
using SharedKernel.Sorting;

namespace CoreService.Application.DTOs;

public sealed class GetThreadPostsRequest : LongKeysetPageRequest
{
    public enum PostSortType
    {
        Id
    }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds ThreadIds { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public SortCriteria<PostSortType>? Sort { get; set; }

    [FromQuery] public bool? Latest { get; set; }
}

public sealed class GetThreadPostsRequestValidator : LongKeysetPageRequestValidator<GetThreadPostsRequest>
{
    public GetThreadPostsRequestValidator()
    {
        RuleForEach(e => e.ThreadIds)
            .GreaterThan(0);
    }
}