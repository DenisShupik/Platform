using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetInternalNotificationCountRequest
{
    [FromQuery] public required bool? IsDelivered { get; init; }
}