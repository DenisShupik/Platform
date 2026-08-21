using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;
using static FileService.Infrastructure.Constants;

namespace FileService.Presentation.Rest;

using Response = NoContent;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='deleteAvatar']/*" />
    public static async Task<Response> DeleteAvatarAsync(
        HttpContext context,
        [FromServices] IAmazonS3 s3Client,
        CancellationToken cancellationToken
    )
    {
        var userId = context.GetRequiredActor().UserId;
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = AvatarBucket,
            Key = $"{userId:D}"
        };

        await s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
        return TypedResults.NoContent();
    }
}
