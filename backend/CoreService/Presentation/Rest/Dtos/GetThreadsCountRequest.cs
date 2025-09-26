using CoreService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadsCountRequest
{
    [FromQuery] public required UserId? CreatedBy { get; init; }
    [FromQuery] public required ThreadStatus? Status { get; init; }
}