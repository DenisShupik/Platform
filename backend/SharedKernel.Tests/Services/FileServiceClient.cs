namespace SharedKernel.Tests.Services;

public sealed class FileServiceClient
{
    private readonly HttpClient _httpClient;

    public FileServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task UploadAvatar(byte[] imageBytes, CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.Add("Content-Type", "image/webp");
        content.Add(fileContent, "file", "user.webp");
        using var response = await _httpClient.PostAsync("api/avatars", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}