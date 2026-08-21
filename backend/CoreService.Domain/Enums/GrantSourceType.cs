namespace CoreService.Domain.Enums;

public enum GrantSourceType : byte
{
    Direct = 1,
    CategoryModeratorAppointment = 2,
    PlatformAdministratorBootstrap = 3,
    PlatformAdministratorAppointment = 4,
    ForumModeratorAppointment = 5
}
