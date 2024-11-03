using SharedKernel;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Sorting;

namespace TopicService.Application.DTOs;

public sealed class GetPostsRequest : LongKeysetPageRequest
{
    public enum PostSortType
    {
        Id
    }
    
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds TopicId { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public SortCriteria<PostSortType>? Sort { get; set; }
}

public sealed class GetPostsRequestValidator : LongKeysetPageRequestValidator<GetPostsRequest>
{
    public GetPostsRequestValidator()
    {
        RuleForEach(e => e.TopicId)
            .GreaterThan(0);
    }
}