using FluentValidation;
using SharedKernel.Infrastructure.Interfaces;

namespace UserService.Presentation.Options;

public sealed class UserServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class UserServiceOptionsValidator : AbstractValidator<UserServiceOptions>
{
    public UserServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}