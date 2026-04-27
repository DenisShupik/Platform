using Amazon.S3;
using Amazon.S3.Model;
using static System.Text.Json.JsonSerializer;

namespace FileService.Infrastructure.Services;

public sealed class S3InitializationService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3InitializationService> _logger;

    public S3InitializationService(IAmazonS3 s3Client, ILogger<S3InitializationService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task InitializeAsync(string bucketName)
    {
        await EnsureBucketExistsAsync(bucketName);
        await EnsureBucketPolicyAsync(bucketName);
    }

    private async Task EnsureBucketExistsAsync(string bucketName)
    {
        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName
            };
            await _s3Client.PutBucketAsync(putBucketRequest);
            _logger.LogInformation("Bucket '{BucketName}' has been created.", bucketName);
        }
        catch (AmazonS3Exception ex) when (
            ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
        {
            _logger.LogInformation("Bucket '{BucketName}' already exists.", bucketName);
        }
    }

    private async Task EnsureBucketPolicyAsync(string bucketName)
    {
        var bucketPolicy = new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = "*",
                    Action = "s3:GetObject",
                    Resource = $"arn:aws:s3:::{bucketName}/*"
                }
            }
        };

        var policyJson = Serialize(bucketPolicy);

        await _s3Client.PutBucketPolicyAsync(
            new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policyJson
            });

        _logger.LogInformation("Policy for bucket '{BucketName}' has been applied.", bucketName);
    }
}