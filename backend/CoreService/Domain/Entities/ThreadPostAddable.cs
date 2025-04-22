using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

public sealed class ThreadPostAddable : IHasThreadId
{
    public ThreadId ThreadId { get; private set; }
    public PostId NextPostId { get; private set; }
    public ThreadStatus Status { get; private set; }
    public UserId CreatedBy { get; private set; }
    public ICollection<Post> Posts { get; private set; } = [];

    public NonThreadOwnerError? AddPost(Post post)
    {
        if (Status == ThreadStatus.Draft)
        {
            if (CreatedBy != post.CreatedBy)
            {
                return new NonThreadOwnerError(ThreadId);
            }

            Status = ThreadStatus.Published;
        }

        post.PostId = NextPostId;
        NextPostId = NextPostId.Increment();
        Posts.Add(post);
        return null;
    }
}