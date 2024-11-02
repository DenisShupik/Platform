using Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public enum PostSortType
{
    CreatedAt
}

public enum SortOrderType
{
    Asc,
    Desc
}

public class SortCriteria<T> where T : Enum
{
    public T By { get; set; }
    public SortOrderType Order { get; set; }

    public static bool TryParse(string? value, IFormatProvider? provider, out SortCriteria<T> result)
    {
        var pair = value?.Split(',');
        result = new SortCriteria<T>
        {
            By = (T)Enum.Parse(typeof(T), pair[0], true),
            Order = (SortOrderType)Enum.Parse(typeof(SortOrderType), pair[1], true),
        };
        return true;
    }
}

public sealed class GetPostsRequest : LongKeysetPageRequest
{
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

public sealed class GetPostsRequestValidator : LongKeysetPageRequestValidator<GetPostsRequest>
{
    public GetPostsRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);
    }
}