using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasRoleId
{
    RoleId RoleId { get; }
}