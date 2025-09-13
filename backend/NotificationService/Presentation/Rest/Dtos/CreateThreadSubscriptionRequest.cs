using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;

namespace NotificationService.Presentation.Rest.Dtos;

[Include(typeof(CreateThreadSubscriptionCommand), PropertyGenerationMode.AsRequired,
    nameof(CreateThreadSubscriptionCommand.Channels))]
public sealed partial class CreateThreadSubscriptionRequestBody;

public sealed class CreateThreadSubscriptionRequest
{
    [FromRoute] public ThreadId ThreadId { get; set; }
    [FromBody] public CreateThreadSubscriptionRequestBody Body { get; set; }
}