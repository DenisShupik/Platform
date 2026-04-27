using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetForumsCountRequest
{
    [FromQuery] public required UserId? CreatedBy { get; init; }
}