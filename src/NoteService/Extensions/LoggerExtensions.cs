namespace NoteService.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Starting the microservice")]
    public static partial void StartingApp(this ILogger logger);
}