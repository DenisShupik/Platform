namespace NotificationService.Application.Authorization;

/// <summary>
/// Business actions guarded by thread-subscription authorization rules.
/// </summary>
public enum ThreadSubscriptionPolicy : byte
{
    Read = 1,
    Manage = 2
}
