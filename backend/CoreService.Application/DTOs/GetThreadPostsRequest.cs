using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
    public long ThreadId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public SortCriteria<PostSortType>? Sort { get; set; }
}

public sealed class GetThreadPostsRequestValidator : LongKeysetPageRequestValidator<GetThreadPostsRequest>
{
    public GetThreadPostsRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);
    }
}