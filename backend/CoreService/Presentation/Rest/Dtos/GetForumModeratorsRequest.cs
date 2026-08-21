using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetForumModeratorsRequest
{
    [FromRoute] public required ForumId ForumId { get; init; }
}
