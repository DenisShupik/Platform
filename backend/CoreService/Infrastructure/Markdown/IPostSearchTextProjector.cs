using CoreService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Markdown;

public interface IPostSearchTextProjector
{
    string Project(PostContent content);
}
