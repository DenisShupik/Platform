using CoreService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class CreateForumRequest
{
    /// <summary>
    /// Название форума
    /// </summary>
    public ForumTitle Title { get; set; }
}