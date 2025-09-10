using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using SharedKernel.TypeGenerator;

namespace CoreService.Domain.Entities;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class CategoryThreadAddable : IHasCategoryId
{
    public ICollection<Thread> Threads { get; private set; } = [];

    public Thread AddThread(ThreadTitle title, UserId createdBy, DateTime createdAt)
    {
        var thread = new Thread(CategoryId, title, createdBy, createdAt);
        Threads.Add(thread);
        return thread;
    }
}