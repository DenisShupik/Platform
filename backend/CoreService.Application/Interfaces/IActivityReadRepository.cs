using CoreService.Application.UseCases;

namespace CoreService.Application.Interfaces;

public interface IActivityReadRepository
{
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetActivitiesPagedQuery request, CancellationToken cancellationToken);
}