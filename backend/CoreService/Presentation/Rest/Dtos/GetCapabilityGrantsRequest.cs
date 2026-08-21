using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetCapabilityGrantsRequest
{
    private static class Defaults
    {
        public const bool IncludeInactive = false;
    }

    [FromQuery] public required AuthorizationScopeType ScopeType { get; init; }
    [FromQuery] public required ForumId? ForumId { get; init; }
    [FromQuery] public required CategoryId? CategoryId { get; init; }
    [FromQuery] public required ThreadId? ThreadId { get; init; }
    [FromQuery] public required bool IncludeInactive { get; init; }
}
