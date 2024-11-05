using FluentValidation;

namespace FileService.Presentation.Options;

public sealed class S3Options
{
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string ServiceURL { get; set; }
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