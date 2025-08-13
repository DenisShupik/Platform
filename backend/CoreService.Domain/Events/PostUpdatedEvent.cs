using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Domain.Events;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.UpdatedBy),
    nameof(Post.UpdatedAt))]
public sealed partial class PostUpdatedEvent;