using UserService.Domain.ValueObjects;

namespace UserService.Domain.Interfaces;

public interface IHasCreateProperties
{
    DateTime CreatedAt { get; }
    UserId CreatedBy { get; }
}