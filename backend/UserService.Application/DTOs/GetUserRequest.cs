using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Application.DTOs;

public sealed class GetUserRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [FromRoute]
    public Guid UserId { get; set; }
}

public sealed class GetUserRequestValidator : AbstractValidator<GetUserRequest>
{
    public GetUserRequestValidator()
    {
        RuleFor(e => e.UserId)
            .NotEmpty();
    }
}