using SharedKernel.Interfaces;
using FluentValidation;

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