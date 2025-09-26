using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetPostIndexRequest
{
    [FromRoute] public required PostId PostId { get; init; }
}