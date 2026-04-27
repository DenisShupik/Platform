using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateCategoryCommand), PropertyGenerationMode.AsRequired, nameof(CreateCategoryCommand.CreatedBy),
    nameof(CreateCategoryCommand.CreatedAt), nameof(CreateCategoryCommand.CreatorRole))]
public sealed partial class CreateCategoryRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class CreateCategoryRequest
{
    [FromBody] public required CreateCategoryRequestBody Body { get; init; }
}