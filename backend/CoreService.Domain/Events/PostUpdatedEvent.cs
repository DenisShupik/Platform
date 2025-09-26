using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Domain.Events;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.UpdatedBy), nameof(Post.UpdatedAt))]
public sealed partial class PostUpdatedEvent;