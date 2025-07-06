using CoreService.Domain.Entities;
using Generator.Attributes; 

namespace CoreService.Domain.Events;

[IncludeAsRequired(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId))]
public sealed partial class PostAddedEvent;