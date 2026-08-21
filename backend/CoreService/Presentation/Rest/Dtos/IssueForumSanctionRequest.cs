using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Generator.Attributes;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(IssueForumSanctionCommand), PropertyGenerationMode.AsRequired,
    nameof(IssueForumSanctionCommand.RequestedBy), nameof(IssueForumSanctionCommand.IssuedAt))]
public sealed partial class IssueForumSanctionRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class IssueForumSanctionRequest
{
    [FromBody] public required IssueForumSanctionRequestBody Body { get; init; }
}
