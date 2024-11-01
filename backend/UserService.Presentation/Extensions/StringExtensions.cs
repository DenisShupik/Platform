using UserService.Application.Events;

namespace UserService.Presentation.Extensions;

public static class StringExtensions
{
    public static Guid? GetGuidFromResourcePath(this UserEvent @event)
    {
        if (string.IsNullOrEmpty(@event.ResourcePath)) return null;
        var segments = @event.ResourcePath.Split('/');
        if (segments.Length > 1 && Guid.TryParse(segments[1], out var result))
        {
            return result;
        }
        return null;
    }
}