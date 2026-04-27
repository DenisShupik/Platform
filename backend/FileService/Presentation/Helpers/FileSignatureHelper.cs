namespace FileService.Presentation.Helpers;

public static class FileSignatureHelper
{
    public static async Task<bool> IsValidWebP(Stream stream, CancellationToken cancellationToken)
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
}