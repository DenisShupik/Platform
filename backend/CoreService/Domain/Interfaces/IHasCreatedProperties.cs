using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasCreatedProperties
{
    DateTime Created { get; set; }
    UserId CreatedBy { get; set; }
}