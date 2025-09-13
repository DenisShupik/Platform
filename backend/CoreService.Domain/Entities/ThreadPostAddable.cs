using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.Status), nameof(Thread.CreatedBy))]
public sealed partial class ThreadPostAddable : IHasThreadId
{
    public ICollection<Post> Posts { get; private set; } = [];

    public OneOf<Post, NonThreadOwnerError> AddPost(PostContent content, UserId createdBy, DateTime createdAt)
    {
        if (Status == ThreadStatus.Draft)
        {
            if (CreatedBy != createdBy)
            {
                return new NonThreadOwnerError(ThreadId);
            }

            Status = ThreadStatus.Published;
        }

        var post = new Post(ThreadId, content, createdBy, createdAt);
        Posts.Add(post);
        return post;
    }
}