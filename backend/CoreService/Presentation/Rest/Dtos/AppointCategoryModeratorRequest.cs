using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class AppointCategoryModeratorRequest
{
    [FromRoute] public required CategoryId CategoryId { get; init; }
    [FromRoute] public required UserId UserId { get; init; }
    [FromQuery] public required DateTime? ValidUntil { get; init; }
}
