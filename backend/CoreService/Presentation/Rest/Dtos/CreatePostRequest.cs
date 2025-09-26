using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class CreatePostRequest
{
    [FromRoute] public required ThreadId ThreadId { get; init; }
    [FromBody] public required CreatePostRequestBody Body { get; init; }
}

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.Content))]
public sealed partial class CreatePostRequestBody;