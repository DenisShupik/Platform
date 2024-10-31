using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetTopicRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long TopicId { get; set; }
}

public sealed class GetTopicRequestValidator : AbstractValidator<GetTopicRequest>
{
    public GetTopicRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);
    }
}