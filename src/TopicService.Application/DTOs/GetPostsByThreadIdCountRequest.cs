using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetPostsByThreadIdCountRequest
{
    /// <summary>
    /// Идентификатор треда
    /// </summary>
    [FromRoute(Name = "threadId")]
    public long ThreadId { get; set; }
}

public sealed class GetPostsByThreadIdCountRequestValidator : AbstractValidator<GetPostsByThreadIdCountRequest>
{
    public GetPostsByThreadIdCountRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);
    }
}