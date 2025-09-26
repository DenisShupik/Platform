using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;
using UserService.Domain.ValueObjects;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetUsersBulkRequest
{
    /// <summary>
    /// Идентификаторы пользователей
    /// </summary>
    [FromRoute]
    public required IdSet<UserId, Guid> UserIds { get; init; }
}