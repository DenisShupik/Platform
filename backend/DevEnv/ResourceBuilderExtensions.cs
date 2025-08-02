using System.Reflection;
using FileService.Infrastructure.Options;
using SharedKernel.Infrastructure.Options;

namespace DevEnv;

public static class ResourceBuilderExtensions
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

    public static IResourceBuilder<T> AddKeycloakOptions<T>(this IResourceBuilder<T> builder, KeycloakOptions options)
        where T : IResourceWithEnvironment => AddOptions(builder, options);

    public static IResourceBuilder<T> AddRabbitMqOptions<T>(this IResourceBuilder<T> builder, RabbitMqOptions options)
        where T : IResourceWithEnvironment => AddOptions(builder, options);
    
    public static IResourceBuilder<T> AddRedisOptions<T>(this IResourceBuilder<T> builder, ValkeyOptions options)
        where T : IResourceWithEnvironment => AddOptions(builder, options);
    
    public static IResourceBuilder<T> AddS3Options<T>(this IResourceBuilder<T> builder, S3Options options)
        where T : IResourceWithEnvironment => AddOptions(builder, options);

   
}