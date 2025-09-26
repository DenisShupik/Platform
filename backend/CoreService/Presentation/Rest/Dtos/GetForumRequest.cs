using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetForumRequest
{
    [FromRoute] public required ForumId ForumId { get; init; }
}