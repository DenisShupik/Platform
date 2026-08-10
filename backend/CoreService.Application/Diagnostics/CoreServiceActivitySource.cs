using System.Diagnostics;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Diagnostics;

public static class CoreServiceActivitySource
{
    public const string SourceName = "Platform.CoreService.Application";

    public const string PreparePostContent = "CreatePost.PrepareContent";
    public const string BeginPostTransaction = "CreatePost.BeginTransaction";
    public const string LoadThreadForPost = "CreatePost.LoadThreadForUpdate";
    public const string HoldThreadLockForPost = "CreatePost.HoldThreadLock";
    public const string AddPostToThread = "CreatePost.AddToThread";
    public const string PublishPostAdded = "CreatePost.PublishEvent";
    public const string CommitPost = "CreatePost.Commit";
    public const string ThreadIdTagName = "forum.thread.id";

    private static readonly ActivitySource Source = new(
        SourceName,
        typeof(CoreServiceActivitySource).Assembly.GetName().Version?.ToString());

    public static Activity? StartCreatePostActivity(string name, ThreadId threadId)
    {
        var activity = Source.StartActivity(name, ActivityKind.Internal);
        activity?.SetTag(ThreadIdTagName, threadId.ToString());
        return activity;
    }
}
