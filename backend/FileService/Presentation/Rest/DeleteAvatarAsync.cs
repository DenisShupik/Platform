using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;
using static FileService.Infrastructure.Constants;

namespace FileService.Presentation.Rest;

public static partial class Api
{
    private static async Task DeleteAvatarAsync(
        HttpContext context,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = context.GetRequiredUserIdRole().UserId;
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = AvatarBucket,
            Key = $"{userId:D}"
        };

        await s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
    }
}