namespace TopicService.Application.DTOs;

public sealed class GetTopicPostsCountResponse
{
    public long TopicId { get; set; }
    public long Count { get; set; }
}