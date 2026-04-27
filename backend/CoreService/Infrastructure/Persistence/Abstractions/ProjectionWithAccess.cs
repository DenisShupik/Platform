namespace CoreService.Infrastructure.Persistence.Abstractions;

public sealed class ProjectionWithAccess<T>
{
    public T Projection { get; set; }
    public bool HasAccess { get; set; }
}