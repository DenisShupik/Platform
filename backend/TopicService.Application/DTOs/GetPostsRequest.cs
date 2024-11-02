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
        result = new SortCriteria<T>
        {
            Order = value[0]=='+'?SortOrderType.Asc:SortOrderType.Desc,
            By = (T)Enum.Parse(typeof(T), value[1..], true),
            
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