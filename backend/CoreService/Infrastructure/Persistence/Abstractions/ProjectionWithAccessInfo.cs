using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Abstractions;

public sealed class ProjectionWithAccessInfo<T>
{
    public T Projection { get; set; }
    public PolicyId ReadPolicyId { get; set; }
    public PolicyValue ReadPolicyValue { get; set; }
    public bool HasGrant { get; set; }
    public bool HasRestriction { get; set; }
}