using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace FileService.Presentation.APIs;

public static class PostApi
{
    private const string AvatarBucket = "avatars";

    public static IEndpointRouteBuilder MapAvatarApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/avatars")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapPost(string.Empty, UploadAvatarAsync).DisableAntiforgery();
        api.MapDelete(string.Empty, DeleteAvatarAsync).DisableAntiforgery();
        return app;
    }

    private static async Task<bool> IsValidWebP(Stream stream, CancellationToken cancellationToken)
    {
        const int headerSize = 12;
        var header = new byte[headerSize];

        var bytesRead = await stream.ReadAsync(header.AsMemory(0, headerSize), cancellationToken);

        if (bytesRead < headerSize)
        {
            return false;
        }

        stream.Position = 0;

        return header[0] == 0x52 &&
               header[1] == 0x49 &&
               header[2] == 0x46 &&
               header[3] == 0x46 &&
               header[8] == 0x57 &&
               header[9] == 0x45 &&
               header[10] == 0x42 &&
               header[11] == 0x50;
    }

    private static async Task<Results<BadRequest<string>, Ok>> UploadAvatarAsync(
        ClaimsPrincipal claimsPrincipal,
        IFormFile? file,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        const long maxFileSize = 1 * 1024 * 1024;
        const string validMimeType = "image/webp";

        if (file is not { Length: > 12 })
        {
            return TypedResults.BadRequest("Invalid file size");
        }

        if (file.ContentType != validMimeType)
        {
            return TypedResults.BadRequest("Only WEBP is allowed");
        }

        await using var stream = file.OpenReadStream();
        if (!await IsValidWebP(stream, cancellationToken)) return TypedResults.BadRequest("Only WEBP is allowed");

        var buffer = new byte[maxFileSize];
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        var objectKey = $"{userId:D}";
        using (var memoryStream = new MemoryStream(buffer, 0, bytesRead))
        {
            var request = new TransferUtilityUploadRequest
            {
                BucketName = AvatarBucket,
                Key = objectKey,
                InputStream = memoryStream,
                ContentType = validMimeType
            };

            var fileTransferUtility = new TransferUtility(s3Client);
            await fileTransferUtility.UploadAsync(request, cancellationToken);
        }

        return TypedResults.Ok();
    }

    private static async Task DeleteAvatarAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = AvatarBucket,
            Key = $"{userId:D}"
        };

        await s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
    }
}