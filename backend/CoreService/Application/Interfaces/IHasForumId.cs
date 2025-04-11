using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IHasForumId
{
    public ForumId ForumId { get; }
}