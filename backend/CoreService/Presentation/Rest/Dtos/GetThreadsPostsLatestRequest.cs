using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetThreadsPostsLatestRequest
{
    [FromRoute] public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
}