using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetCategoriesPostsCountRequest
{
    [FromRoute] public required IdSet<CategoryId, Guid> CategoryIds { get; init; }
}