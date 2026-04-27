using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasThreadId
{
    ThreadId ThreadId { get; }
}