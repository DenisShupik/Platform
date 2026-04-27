namespace Shared.Infrastructure.Interfaces;

public interface IDbOptions
{
    string ReadonlyConnectionString { get; set; }
    string WritableConnectionString { get; set; }
}