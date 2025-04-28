using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.Domain.Interfaces;

public interface IHasCreateProperties
{
    DateTime CreatedAt { get; }
    UserId CreatedBy { get; }
}