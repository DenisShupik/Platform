using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Abstractions;

public interface IHasCreatedProperties
{
    DateTime Created { get; set; }
    UserId CreatedBy { get; set; }
}