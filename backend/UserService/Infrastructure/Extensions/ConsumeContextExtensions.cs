using System.Text;
using System.Text.Json;
using MassTransit;
using UserService.Infrastructure.Events;

namespace UserService.Infrastructure.Extensions;

public static class ConsumeContextExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static TValue GetMessage<TValue>(this ConsumeContext<UserEvent> context)
    {
        var rawMessageBytes = context.ReceiveContext.GetBody();
        var json = Encoding.UTF8.GetString(rawMessageBytes.ToArray());
        return JsonSerializer.Deserialize<TValue>(json, JsonOptions)!;
    }
}