using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;
using SharedKernel.Sorting;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryThreadsRequest : LongKeysetPageRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GetCategoryThreadsRequestSortType
    {
        Activity
    }
    
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
    
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromQuery]
    public SortCriteria<GetCategoryThreadsRequestSortType>? Sort { get; set; }
}

public sealed class GetCategoryThreadsRequestValidator : LongKeysetPageRequestValidator<GetCategoryThreadsRequest>
{
    public GetCategoryThreadsRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}