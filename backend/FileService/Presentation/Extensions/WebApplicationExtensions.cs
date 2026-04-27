using FileService.Infrastructure.Services;

namespace FileService.Presentation.Extensions;

public static class WebApplicationExtensions
{
    public static async Task EnsureS3Init(this WebApplication app, string bucketName)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var s3InitializationService = scope.ServiceProvider.GetRequiredService<S3InitializationService>();
        await s3InitializationService.InitializeAsync(bucketName);
    }
}