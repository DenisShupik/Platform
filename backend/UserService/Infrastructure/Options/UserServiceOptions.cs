using FluentValidation;
using Shared.Infrastructure.Interfaces;

namespace UserService.Infrastructure.Options;

public sealed class UserServiceOptions : IDbOptions
{
    public string ReadonlyConnectionString { get; set; } = null!;
    public string WritableConnectionString { get; set; } = null!;
}

public sealed class UserServiceOptionsValidator : AbstractValidator<UserServiceOptions>
{
    public UserServiceOptionsValidator()
    {
        RuleFor(e => e.ReadonlyConnectionString)
            .NotEmpty();

        RuleFor(e => e.WritableConnectionString)
            .NotEmpty();
    }
}