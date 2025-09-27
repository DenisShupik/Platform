using CoreService.Domain.Enums;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class CategoryThreadAddable : IHasCategoryId
{
    public ICollection<Thread> Threads { get; private set; } = [];

    public Thread AddThread(ThreadTitle title, UserId createdBy, DateTime createdAt, AccessLevel accessLevel)
    {
        var thread = new Thread(CategoryId, title, createdBy, createdAt, accessLevel);
        Threads.Add(thread);
        return thread;
    }
}