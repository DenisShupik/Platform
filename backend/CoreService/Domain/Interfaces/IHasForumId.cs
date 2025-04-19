using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasForumId
{
    public ForumId ForumId { get; }
}