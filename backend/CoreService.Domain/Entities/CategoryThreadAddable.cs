using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class CategoryThreadAddable : IHasCategoryId
{
    public CategoryId CategoryId { get; }
    public ICollection<Thread> Threads { get; private set; } = [];

    public Thread AddThread(ThreadTitle title, UserId createdBy, DateTime createdAt)
    {
        var thread = new Thread(CategoryId, title, createdBy, createdAt);
        Threads.Add(thread);
        return thread;
    }
}