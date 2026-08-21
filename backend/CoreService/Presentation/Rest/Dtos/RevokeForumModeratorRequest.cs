using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class RevokeForumModeratorRequest
{
    [FromRoute] public required ForumId ForumId { get; init; }
    [FromRoute] public required UserId UserId { get; init; }
}
