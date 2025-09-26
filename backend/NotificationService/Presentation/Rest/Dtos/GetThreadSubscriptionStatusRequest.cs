using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadSubscriptionStatusRequest
{
    [FromRoute] public required ThreadId ThreadId { get; init; }
}