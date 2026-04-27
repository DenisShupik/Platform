using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Domain.Entities;

/// <summary>
/// Тема
/// </summary>
public sealed class Thread
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; private set; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; private set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public ThreadTitle Title { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public ThreadState State { get; private set; }

    /// <summary>
    /// Количество сообщений в теме
    /// </summary>
    public Count PostCount { get; private set; }

    /// <summary>
    /// Идентификатор последнего заголовочного поста в теме
    /// </summary>
    public PostId? LastHeaderPostId { get; private set; }

    internal Thread(CategoryId categoryId, ThreadTitle title, UserId createdBy, DateTime createdAt)
    {
        ThreadId = ThreadId.From(Guid.CreateVersion7());
        CategoryId = categoryId;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        State = ThreadState.Draft;
        PostCount = Count.Default;
        LastHeaderPostId = null;
    }

    public Result<Success, ThreadNotInStateError, ThreadMustContainPostsError> RequestApproval()
    {
        if (State != ThreadState.Draft) return new ThreadNotInStateError(ThreadState.Draft);
        if (PostCount < 1) return new ThreadMustContainPostsError();
        State = ThreadState.PendingApproval;
        return Success.Instance;
    }

    public Result<Success, ThreadNotInStateError> ApproveThread()
    {
        if (State != ThreadState.PendingApproval) return new ThreadNotInStateError(ThreadState.PendingApproval);
        State = ThreadState.Approved;
        return Success.Instance;
    }

    public Result<Success, ThreadNotInStateError> RejectThread()
    {
        if (State != ThreadState.PendingApproval) return new ThreadNotInStateError(ThreadState.PendingApproval);
        State = ThreadState.Draft;
        return Success.Instance;
    }

    public Result<Post, ThreadLockedByStateError, NonThreadOwnerError, PostLimitReachedError> AddPost(
        PostContent content, UserId createdBy, DateTime createdAt)
    {
        if (State == ThreadState.PendingApproval) return new ThreadLockedByStateError(State);

        var newCount = PostCount.Increment();

        if (State == ThreadState.Draft)
        {
            if (CreatedBy != createdBy) return new NonThreadOwnerError();

            if (newCount > 5) return new PostLimitReachedError();
        }

        PostCount = newCount;
        var post = new Post(ThreadId, content, createdBy, createdAt);
        if (State == ThreadState.Draft) LastHeaderPostId = post.PostId;

        return post;
    }

    public Result<Success, ThreadLockedByStateError, NonThreadOwnerError, ApprovedHeaderPostDeletionForbiddenError>
        DeletePost(Post post, UserId deletedBy)
    {
        if (State == ThreadState.PendingApproval) return new ThreadLockedByStateError(State);
        if (State == ThreadState.Draft && CreatedBy != deletedBy) return new NonThreadOwnerError();
        if (State == ThreadState.Approved && LastHeaderPostId.Value > post.PostId)
            return new ApprovedHeaderPostDeletionForbiddenError();

        PostCount = PostCount.Decrement();

        return Success.Instance;
    }

    public Result<Success, ThreadLockedByStateError, NonThreadOwnerError, InsufficientRoleToEditHeaderPostError>
        CanUpdatePost(PostId postId, UserId updatedBy, Role updaterRole)
    {
        if (State == ThreadState.PendingApproval) return new ThreadLockedByStateError(State);
        if (State == ThreadState.Draft && CreatedBy != updatedBy) return new NonThreadOwnerError();
        if (State == ThreadState.Approved && postId <= LastHeaderPostId.Value && updaterRole < Role.Moderator)
            return new InsufficientRoleToEditHeaderPostError();

        return Success.Instance;
    }
}