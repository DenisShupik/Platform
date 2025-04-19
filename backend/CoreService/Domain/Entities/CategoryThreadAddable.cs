using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class CategoryThreadAddable : IHasCategoryId
{
    public CategoryId CategoryId { get; }
    public ICollection<Thread> Threads { get; private set; } = [];

    public void AddThread(Thread thread)
    {
        Threads.Add(thread);
    }
}