using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Presentation.Generator;

namespace NotificationService.Presentation.Rest.Dtos;

[Include(typeof(CreateThreadSubscriptionCommand), PropertyGenerationMode.AsRequired,
    nameof(CreateThreadSubscriptionCommand.Channels))]
public sealed partial class CreateThreadSubscriptionRequestBody;

[GenerateBind]
public sealed partial class CreateThreadSubscriptionRequest
{
    [FromRoute] public required ThreadId ThreadId { get; init; }
    [FromBody] public required CreateThreadSubscriptionRequestBody Body { get; init; }
}