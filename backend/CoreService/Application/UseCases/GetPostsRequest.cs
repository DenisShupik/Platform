using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Paging;

namespace CoreService.Application.UseCases;

public sealed class GetPostsRequest : LongKeysetPageRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterType
    {
        CategoryLatest,
        ThreadLatest
    }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public LongIds? Ids { get; set; }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public FilterType? Filter { get; set; }
}

public sealed class GetPostsRequestValidator : LongKeysetPageRequestValidator<GetPostsRequest>
{
    public GetPostsRequestValidator()
    {
        RuleForEach(e => e.Ids)
            .GreaterThan(0)
            .When(e => e.Ids != null);

        RuleFor(e => e.Filter)
            .IsInEnum()
            .When(e => e.Filter != null);
    }
}