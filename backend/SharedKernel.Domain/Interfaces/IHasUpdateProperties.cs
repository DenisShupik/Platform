using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.Domain.Interfaces;

public interface IHasUpdateProperties
{
    DateTime UpdatedAt { get; }
    UserId UpdatedBy { get; }
}