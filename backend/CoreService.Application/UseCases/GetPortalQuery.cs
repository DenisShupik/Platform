using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

using GetPortalQueryResult = PortalDto;

public sealed class GetPortalQuery : IQuery<GetPortalQueryResult>;

public sealed class GetPortalQueryHandler : IQueryHandler<GetPortalQuery, GetPortalQueryResult>
{
    private readonly IPortalReadRepository _portalReadRepository;

    public GetPortalQueryHandler(
        IPortalReadRepository portalReadRepository
    )
    {
        _portalReadRepository = portalReadRepository;
    }
    
    public Task<GetPortalQueryResult> HandleAsync(GetPortalQuery query, CancellationToken cancellationToken)
    {
        return _portalReadRepository.GetAsync(cancellationToken);
    }
}