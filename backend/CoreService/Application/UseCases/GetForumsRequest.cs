using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;
using SharedKernel.Sorting;

namespace CoreService.Application.UseCases;

public sealed class GetForumsRequest : LongKeysetPageRequest
{
    public enum SortType
    {
        latestPost
    }
    
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public SortCriteria<SortType>? Sort { get; set; }
}

public sealed class GetForumsRequestValidator : LongKeysetPageRequestValidator<GetForumsRequest>
{
}