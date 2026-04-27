using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasCategoryId
{
    CategoryId CategoryId { get; }
}