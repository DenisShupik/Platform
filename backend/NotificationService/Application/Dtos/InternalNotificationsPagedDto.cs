using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Dtos;

public sealed class InternalNotificationsPagedDto
{
    public IReadOnlyList<InternalNotificationDto> Notifications { get; set; }
    public Dictionary<ThreadId, ThreadTitle> Threads { get; set; }
    public Dictionary<UserId, Username> Users { get; set; }

    /// <include file="../../Documentation/Api.en.xml" path="docs/member[@key='InternalNotificationsPagedDto.TotalCount']/*" />
    public Count TotalCount { get; set; }
}
