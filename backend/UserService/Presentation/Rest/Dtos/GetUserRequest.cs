using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator;
using UserService.Domain.ValueObjects;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetUserRequest
{
    [FromRoute] public required UserId UserId { get; init; }
}