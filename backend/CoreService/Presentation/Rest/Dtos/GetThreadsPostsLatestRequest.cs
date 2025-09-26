using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions;
using Shared.Presentation.Generator;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadsPostsLatestRequest
{
    [FromRoute] public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
}