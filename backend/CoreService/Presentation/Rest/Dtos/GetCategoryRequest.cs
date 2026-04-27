using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetCategoryRequest
{
    [FromRoute] public required CategoryId CategoryId { get; init; }
}