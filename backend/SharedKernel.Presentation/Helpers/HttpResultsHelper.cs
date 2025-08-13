using Microsoft.Extensions.Logging;

namespace SharedKernel.Presentation.Helpers;

internal static partial class HttpResultsHelper
{
    internal static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information,
            "Setting HTTP status code {StatusCode}.",
            EventName = "WritingResultAsStatusCode")]
        public static partial void WritingResultAsStatusCode(ILogger logger, int statusCode);
    }
}