using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetCategoriesThreadsCountRequest
{
    private static class Defaults
    {
        public static readonly bool IncludeDraft = false;
    }

    [FromRoute] public required IdSet<CategoryId, Guid> CategoryIds { get; init; }
    [FromQuery] public required bool IncludeDraft { get; init; }
}