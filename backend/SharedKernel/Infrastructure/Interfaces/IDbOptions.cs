namespace SharedKernel.Infrastructure.Interfaces;

public interface IDbOptions
{
    string ConnectionString { get; set; }
}