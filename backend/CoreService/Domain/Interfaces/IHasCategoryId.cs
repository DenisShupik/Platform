using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasCategoryId
{
    public CategoryId CategoryId { get; }
}