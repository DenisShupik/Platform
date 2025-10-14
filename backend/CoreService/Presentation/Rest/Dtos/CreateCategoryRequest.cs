using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class CreateCategoryRequest
{
    [FromBody] public required CreateCategoryRequestBody Body { get; init; }
}