using System.Reflection;
using FileService.Infrastructure.Options;
using Shared.Infrastructure.Options;

namespace DevEnv.Extensions;

internal static class ResourceBuilderExtensions
{
    private static IResourceBuilder<T> AddOptions<T, TOptions>(IResourceBuilder<T> builder, TOptions options)
        where T : IResourceWithEnvironment
    {
        var optionsType = typeof(TOptions);
        var sectionName = optionsType.Name;

        foreach (var property in optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(options)?.ToString();
            builder.WithEnvironment($"{sectionName}:{property.Name}", value);
        }

        return builder;
    }

    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {
        public IResourceBuilder<T> AddKeycloakOptions(KeycloakOptions options) => AddOptions(builder, options);
        public IResourceBuilder<T> AddRabbitMqOptions(RabbitMqOptions options) => AddOptions(builder, options);
        public IResourceBuilder<T> AddRedisOptions(ValkeyOptions options) => AddOptions(builder, options);
        public IResourceBuilder<T> AddS3Options(S3Options options) => AddOptions(builder, options);
    }
}