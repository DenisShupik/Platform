using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator.Attributes;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetCategoriesThreadsCountRequest
{
    [FromRoute] public required IdSet<CategoryId, Guid> CategoryIds { get; init; }
    [FromQuery] public required ThreadState? State { get; init; }
}