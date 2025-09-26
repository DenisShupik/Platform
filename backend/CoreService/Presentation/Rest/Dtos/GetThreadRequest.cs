using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadRequest
{
    [FromRoute] public required ThreadId ThreadId { get; init; }
}