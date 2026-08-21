using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateForumCommand), PropertyGenerationMode.AsRequired,
    nameof(CreateForumCommand.CreatedAt), nameof(CreateForumCommand.RequestedBy))]
public sealed partial class CreateForumRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class CreateForumRequest
{
    [FromBody] public required CreateForumRequestBody Body { get; init; }
}
