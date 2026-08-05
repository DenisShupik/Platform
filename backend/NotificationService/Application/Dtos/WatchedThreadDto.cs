using NotificationService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.Dtos;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsPublic, nameof(ThreadSubscription.ThreadId))]
public sealed partial class WatchedThreadDto;
