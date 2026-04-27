using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasForumId
{
    ForumId ForumId { get; }
}