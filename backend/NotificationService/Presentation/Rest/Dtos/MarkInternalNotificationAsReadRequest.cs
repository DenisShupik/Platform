using Microsoft.AspNetCore.Mvc;
using NotificationService.Domain.ValueObjects;
using Shared.Presentation.Generator;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class MarkInternalNotificationAsReadRequest
{
    [FromRoute] public required NotifiableEventId NotifiableEventId { get; init; }
}