using Shared.Domain.ValueObjects;

namespace ApiGateway.Infrastructure.Interfaces;

public interface IOpenApiAggregatorService
{
    ValueTask<string> GetOpenApiJson(Locale locale, CancellationToken cancellationToken);
}
