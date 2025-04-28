using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;
using OneOf;

namespace CoreService.Domain.Entities;

public sealed class ThreadPostAddable : IHasThreadId
{
    public ThreadId ThreadId { get; private set; }
    public ThreadStatus Status { get; private set; }
    public UserId CreatedBy { get; private set; }
    public PostId NextPostId { get; private set; }
    public ICollection<Post> Posts { get; private set; } = [];

    public OneOf<NonThreadOwnerError, Post> AddPost(PostContent content, UserId createdBy, DateTime createdAt)
    {
        if (Status == ThreadStatus.Draft)
        {
            if (CreatedBy != createdBy)
            {
                return new NonThreadOwnerError(ThreadId);
            }

            Status = ThreadStatus.Published;
        }

        var post = new Post(ThreadId, NextPostId, content, createdBy, createdAt);
        NextPostId = NextPostId.Increment();
        Posts.Add(post);
        return post;
    }
}