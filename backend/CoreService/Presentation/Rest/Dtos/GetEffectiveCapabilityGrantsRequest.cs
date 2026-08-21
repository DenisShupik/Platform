using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetEffectiveCapabilityGrantsRequest
{
    [FromRoute] public required UserId UserId { get; init; }
    [FromQuery] public required AuthorizationScopeType ScopeType { get; init; }
    [FromQuery] public required ForumId? ForumId { get; init; }
    [FromQuery] public required CategoryId? CategoryId { get; init; }
    [FromQuery] public required ThreadId? ThreadId { get; init; }
}
