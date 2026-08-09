using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;
using UserService.Application.UseCases;

namespace UserService.Presentation.Rest.Dtos;

[Omit(
    typeof(ChangeCurrentUserLocaleCommand),
    PropertyGenerationMode.AsRequired,
    nameof(ChangeCurrentUserLocaleCommand.UserId))]
public sealed partial class ChangeCurrentUserLocaleRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class ChangeCurrentUserLocaleRequest
{
    [FromBody] public required ChangeCurrentUserLocaleRequestBody Body { get; init; }
}
