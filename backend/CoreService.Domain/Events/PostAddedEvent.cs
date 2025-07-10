using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Domain.Events;

[IncludeAsRequired(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.CreatedBy),nameof(Post.CreatedAt))]
public sealed partial class PostAddedEvent;