using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class RevokeForumSanctionRequest
{
    [FromRoute] public required ForumSanctionId ForumSanctionId { get; init; }
}
