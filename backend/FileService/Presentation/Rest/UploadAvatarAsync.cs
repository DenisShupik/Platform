using Amazon.S3;
using Amazon.S3.Model;
using FileService.Presentation.Errors;
using FileService.Presentation.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;
using static FileService.Infrastructure.Constants;

namespace FileService.Presentation.Rest;

using Response = Results<
    NoContent,
    BadRequest<InvalidAvatarFileSizeError>,
    BadRequest<InvalidAvatarFileTypeError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='uploadAvatar']/*" />
    [RequestSizeLimit(AvatarMaxFileSize)]
    public static async Task<Response> UploadAvatarAsync(
        HttpContext context,
        IFormFile file,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = context.GetRequiredUserIdRole().UserId;

        const long minimumFileSize = 13;
        if (file.Length < minimumFileSize || file.Length > AvatarMaxFileSize)
        {
            return TypedResults.BadRequest(new InvalidAvatarFileSizeError(
                minimumFileSize,
                AvatarMaxFileSize,
                file.Length));
        }

        if (file.ContentType != ValidMimeType)
        {
            return TypedResults.BadRequest(new InvalidAvatarFileTypeError(ValidMimeType));
        }

        await using var stream = file.OpenReadStream();
        if (!await FileSignatureHelper.IsValidWebP(stream, cancellationToken))
            return TypedResults.BadRequest(new InvalidAvatarFileTypeError(ValidMimeType));

        var objectKey = $"{userId:D}";

        var putRequest = new PutObjectRequest
        {
            BucketName = AvatarBucket,
            Key = objectKey,
            InputStream = stream,
            ContentType = ValidMimeType
        };

        await s3Client.PutObjectAsync(putRequest, cancellationToken);
        return TypedResults.NoContent();
    }
}
