using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace FileService.Presentation.Rest;

public static partial class Api
{
    private const string AvatarBucket = "avatars";
    private const long AvatarMaxFileSize = 1 * 1024 * 1024;
    private const string ValidMimeType = "image/webp";

    private static IEndpointRouteBuilder AvatarApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/avatars")
            .WithTags(nameof(AvatarApi))
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapPost(string.Empty, UploadAvatarAsync).DisableAntiforgery();
        api.MapDelete(string.Empty, DeleteAvatarAsync).DisableAntiforgery();
        return app;
    }
    
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.AvatarApi();

        return app;
    }
}