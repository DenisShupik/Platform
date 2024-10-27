using Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetPostsByThreadIdRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор треда
    /// </summary>
    [FromRoute(Name = "threadId")]
    public long ThreadId { get; set; }
}

public sealed class GetPostsByThreadIdRequestValidator : LongKeysetPageRequestValidator<GetPostsByThreadIdRequest>
{
    public GetPostsByThreadIdRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);
    }
}