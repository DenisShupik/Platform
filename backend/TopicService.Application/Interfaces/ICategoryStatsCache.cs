namespace TopicService.Application.Interfaces;

public interface ICategoryStatsCache
{
    ValueTask<long> GetTopicCountAsync(long categoryId, CancellationToken cancellationToken);
    ValueTask UpdateTopicCountAsync(long categoryId);
}