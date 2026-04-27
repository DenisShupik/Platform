using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.None)]
public sealed partial class GetUserRequest
{
    [FromRoute] public required UserId UserId { get; init; }
}