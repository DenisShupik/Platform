using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.None)]
public sealed partial class GetUsersBulkRequest
{
    /// <summary>
    /// User identifiers
    /// </summary>
    [FromRoute]
    public required IdSet<UserId, Guid> UserIds { get; init; }
}
