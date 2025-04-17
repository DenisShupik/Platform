using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class ThreadPostAddable : IHasThreadId
{
    public ThreadId ThreadId { get; private set; }
    public long PostIdSeq { get; private set; }
    public ICollection<Post> Posts { get; private set; } = [];

    public void AddPost(Post post)
    {
        PostIdSeq += 1;
        post.PostId = PostId.From(PostIdSeq);
        Posts.Add(post);
    }
}