using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasThreadId
{
    public ThreadId ThreadId { get; }
}