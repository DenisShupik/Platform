using UserService.Domain.ValueObjects;

namespace UserService.Domain.Interfaces;

public interface IHasCreateProperties
{
    UserId CreatedBy { get; }
    DateTime CreatedAt { get; }
}