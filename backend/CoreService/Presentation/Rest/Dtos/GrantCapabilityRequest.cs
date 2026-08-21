using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(GrantCapabilityCommand), PropertyGenerationMode.AsRequired,
    nameof(GrantCapabilityCommand.RequestedBy), nameof(GrantCapabilityCommand.GrantedAt))]
public sealed partial class GrantCapabilityRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GrantCapabilityRequest
{
    [FromBody] public required GrantCapabilityRequestBody Body { get; init; }
}
