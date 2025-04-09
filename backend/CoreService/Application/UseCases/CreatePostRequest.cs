using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.UseCases;

public sealed class CreatePostRequest
{
    public sealed class FromBody
    {
        /// <summary>
        /// Содержимое сообщения
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public ThreadId ThreadId { get; set; }

    [FromBody] public FromBody Body { get; set; }
}

public sealed class CretePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CretePostRequestValidator()
    {

        RuleFor(e => e.Body.Content)
            .NotEmpty()
            .MaximumLength(Post.ContentMaxLength);
    }
}