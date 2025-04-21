using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.Domain.Interfaces;

public interface IHasCreateProperties
{
    DateTime CreatedAt { get; set; }
    UserId CreatedBy { get; set; }
}