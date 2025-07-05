using UserService.Domain.ValueObjects;

namespace UserService.Domain.Interfaces;

public interface IHasUpdateProperties
{
    DateTime UpdatedAt { get; }
    UserId UpdatedBy { get; }
}