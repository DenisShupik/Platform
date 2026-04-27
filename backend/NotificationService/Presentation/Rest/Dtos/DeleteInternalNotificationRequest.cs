using Microsoft.AspNetCore.Mvc;
using NotificationService.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class DeleteInternalNotificationRequest
{
    [FromRoute] public required NotifiableEventId NotifiableEventId { get; init; }
}