using FluentValidation;

namespace FileService.Infrastructure.Options;

public sealed class S3Options
{
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string ServiceURL { get; init; }
}

public sealed class S3OptionsValidator : AbstractValidator<S3Options>
{
    public S3OptionsValidator()
    {
        RuleFor(options => options.AccessKey)
            .NotEmpty();

        RuleFor(options => options.SecretKey)
            .NotEmpty();

        RuleFor(options => options.ServiceURL)
            .NotEmpty();
    }
}