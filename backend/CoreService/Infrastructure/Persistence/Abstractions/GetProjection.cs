using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Abstractions;

public sealed class GetProjection<T>
{
    public T Projection { get; set; }
    public PolicyId AccessPolicyId { get; set; }
    public PolicyValue AccessPolicyValue { get; set; }
    public bool HasGrant { get; set; }
    public bool HasRestriction { get; set; }
}