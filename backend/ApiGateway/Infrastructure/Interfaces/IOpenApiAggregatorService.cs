namespace ApiGateway.Infrastructure.Interfaces;

public interface IOpenApiAggregatorService
{
    ValueTask<string> GetOpenApiJson(CancellationToken cancellationToken);
}