using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetThreadsCountRequest
{
    [FromQuery] public required UserId? CreatedBy { get; init; }
    [FromQuery] public required ThreadState? Status { get; init; }
}