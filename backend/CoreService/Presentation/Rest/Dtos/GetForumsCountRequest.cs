using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetForumsCountRequest
{
    [FromQuery] public required UserId? CreatedBy { get; init; }
}