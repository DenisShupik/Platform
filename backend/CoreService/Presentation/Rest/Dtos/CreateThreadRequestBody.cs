using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateThreadCommand), PropertyGenerationMode.AsRequired, nameof(CreateThreadCommand.CreatedBy),
    nameof(CreateThreadCommand.CreatedAt), nameof(CreateThreadCommand.CreatorRole))]
public sealed partial class CreateThreadRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class CreateThreadRequest
{
    [FromBody] public required CreateThreadRequestBody Body { get; init; }
}