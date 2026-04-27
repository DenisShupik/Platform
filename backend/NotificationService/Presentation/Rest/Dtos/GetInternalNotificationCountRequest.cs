using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetInternalNotificationCountRequest
{
    [FromQuery] public required bool? IsDelivered { get; init; }
}