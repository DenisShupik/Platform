using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class UpdatePostRequest
{
    [FromRoute] public required PostId PostId { get; init; }
    [FromBody] public required UpdatePostRequestBody Body { get; init; }
}

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.Content), nameof(Post.RowVersion))]
public sealed partial class UpdatePostRequestBody;