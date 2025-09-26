using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetForumsCategoriesCountRequest
{
    [FromRoute] public required IdSet<ForumId, Guid> ForumIds { get; init; }
}