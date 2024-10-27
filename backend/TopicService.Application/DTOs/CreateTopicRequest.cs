using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreateTopicRequest
{
    /// <summary>
    /// Название темы
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateTopicRequestValidator : AbstractValidator<CreateTopicRequest>
{
    public CreateTopicRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(Topic.TitleMaxLength);
    }
}