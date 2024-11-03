using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;
using SharedKernel.Sorting;

namespace TopicService.Application.DTOs;

public sealed class GetTopicPostsRequest : LongKeysetPageRequest
{
    public enum PostSortType
    {
        Id
    }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long TopicId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public SortCriteria<PostSortType>? Sort { get; set; }
}

public sealed class GetTopicPostsRequestValidator : LongKeysetPageRequestValidator<GetTopicPostsRequest>
{
    public GetTopicPostsRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);
    }
}