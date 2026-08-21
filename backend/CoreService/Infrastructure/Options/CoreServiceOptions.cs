using FluentValidation;
using Shared.Infrastructure.Interfaces;

namespace CoreService.Infrastructure.Options;

public sealed class CoreServiceOptions : IDbOptions
{
    public string ReadonlyConnectionString { get; set; } = null!;
    public string WritableConnectionString { get; set; } = null!;
    public Uri UserServiceGrpcAddress { get; set; } = null!;
}

public sealed class CoreServiceOptionsValidator : AbstractValidator<CoreServiceOptions>
{
    public CoreServiceOptionsValidator()
    {
        RuleFor(e => e.ReadonlyConnectionString)
            .NotEmpty();

        RuleFor(e => e.WritableConnectionString)
            .NotEmpty();

        RuleFor(e => e.UserServiceGrpcAddress)
            .NotNull()
            .Must(address => address.IsAbsoluteUri);

    }
}
