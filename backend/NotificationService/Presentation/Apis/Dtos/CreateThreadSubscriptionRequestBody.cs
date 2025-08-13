using Generator.Attributes;
using NotificationService.Application.UseCases;

namespace NotificationService.Presentation.Apis.Dtos;

[Include(typeof(CreateThreadSubscriptionCommand), PropertyGenerationMode.AsRequired,
    nameof(CreateThreadSubscriptionCommand.Channels))]
public sealed partial class CreateThreadSubscriptionRequestBody;