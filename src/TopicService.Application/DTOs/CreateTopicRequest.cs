using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class CreateTopicRequest
{
    /// <summary>
    /// Название темы
    /// </summary>
    [FromBody]
    public string Title { get; set; }
}

public sealed class CreateTopicRequestValidator : AbstractValidator<CreateTopicRequest>
{
    public CreateTopicRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(256);
    }
}