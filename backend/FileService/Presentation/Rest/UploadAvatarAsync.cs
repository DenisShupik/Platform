using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using FileService.Presentation.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace FileService.Presentation.Rest;

public static partial class Api
{
    [RequestSizeLimit(AvatarMaxFileSize)]
    private static async Task<Results<NoContent, BadRequest<string>, InternalServerError>> UploadAvatarAsync(
        ClaimsPrincipal claimsPrincipal,
        IFormFile file,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();

        if (file.Length <= 12)
        {
            return TypedResults.BadRequest("Invalid file size");
        }

        if (file.ContentType != ValidMimeType)
        {
            return TypedResults.BadRequest("Only WEBP is allowed");
        }

        await using var stream = file.OpenReadStream();
        if (!await FileSignatureHelper.IsValidWebP(stream, cancellationToken))
            return TypedResults.BadRequest("Only WEBP is allowed");

        var objectKey = $"{userId:D}";

        var putRequest = new PutObjectRequest
        {
            BucketName = AvatarBucket,
            Key = objectKey,
            InputStream = stream,
            ContentType = ValidMimeType
        };

        try
        {
            await s3Client.PutObjectAsync(putRequest, cancellationToken);
            return TypedResults.NoContent();
        }
        catch
        {
            return TypedResults.InternalServerError();
        }
    }
}