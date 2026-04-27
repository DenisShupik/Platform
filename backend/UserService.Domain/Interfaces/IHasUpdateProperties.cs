using Shared.Domain.ValueObjects;

namespace UserService.Domain.Interfaces;

public interface IHasUpdateProperties
{
    UserId UpdatedBy { get; }
    DateTime UpdatedAt { get; }
}