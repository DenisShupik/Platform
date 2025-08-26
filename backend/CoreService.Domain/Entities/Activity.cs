using Generator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public abstract class Activity
{
    public required UserId OccurredBy { get; init; }
    public required DateTime OccurredAt { get; init; }
}

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId))]
[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.PostId))]
public sealed partial class PostAddedActivity : Activity;