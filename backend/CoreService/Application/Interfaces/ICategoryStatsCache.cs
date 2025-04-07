namespace CoreService.Application.Interfaces;

public interface ICategoryStatsCache
{
    ValueTask<long> GetThreadCountAsync(long categoryId, CancellationToken cancellationToken);
    ValueTask UpdateThreadCountAsync(long categoryId);
}