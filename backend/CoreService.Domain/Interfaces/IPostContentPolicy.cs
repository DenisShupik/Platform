using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IPostContentPolicy
{
    bool IsAllowed(PostContent content);
}
