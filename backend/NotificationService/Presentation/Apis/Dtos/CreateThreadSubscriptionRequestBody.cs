using Generator.Attributes;
using NotificationService.Application.UseCases;

namespace NotificationService.Presentation.Apis.Dtos;

[IncludeAsRequired(typeof(CreateThreadSubscriptionCommand), nameof(CreateThreadSubscriptionCommand.Channels))]
public sealed partial class CreateThreadSubscriptionRequestBody;