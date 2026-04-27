using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.Content))]
public sealed partial class CreatePostRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class CreatePostRequest
{
    [FromRoute] public required ThreadId ThreadId { get; init; }
    [FromBody] public required CreatePostRequestBody Body { get; init; }
}

