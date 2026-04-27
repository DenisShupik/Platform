using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;

namespace IntegrationTests;

public static class TestRequests
{
    public static readonly CreateForumRequestBody CreateForum = new()
    {
        Title = ForumTitle.From("Тестовый форум")
    };

    public static CreateCategoryRequestBody CreateCategory(ForumId forumId) => new()
    {
        ForumId = forumId,
        Title = CategoryTitle.From("Тестовый раздел")
    };

    public static CreateThreadRequestBody CreateThread(CategoryId categoryId, string title = "Тестовая тема") => new()
    {
        CategoryId = categoryId,
        Title = ThreadTitle.From(title)
    };

    public static readonly CreatePostRequestBody CreatePost = new()
    {
        Content = PostContent.From("Тестовое сообщение")
    };
    
    public static readonly CreatePostRequestBody CreateHeaderPost = new()
    {
        Content = PostContent.From("Заглавное сообщение темы")
    };
}
