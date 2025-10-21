using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetPoliciesBulkRequest
{
    [FromRoute] public required IdSet<PolicyId, Guid> PolicyIds { get; init; }
}