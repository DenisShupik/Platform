using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IHasThreadId
{
    public ThreadId ThreadId { get; }
}