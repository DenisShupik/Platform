using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface ICategoryStatsCache
{
    ValueTask<long> GetThreadCountAsync(CategoryId categoryId, CancellationToken cancellationToken);
    ValueTask UpdateThreadCountAsync(CategoryId categoryId);
}