namespace Shared.Presentation.Authorization;

public static class AuthenticationPolicies
{
    public const string PublicApi = nameof(PublicApi);
    public const string InternalApi = nameof(InternalApi);
    public const string CoreServiceInternalApi = nameof(CoreServiceInternalApi);
    public const string NotificationServiceInternalApi = nameof(NotificationServiceInternalApi);
    public const string ProvisioningServiceInternalApi = nameof(ProvisioningServiceInternalApi);
}
