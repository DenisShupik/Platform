using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId), nameof(Thread.Status),
    nameof(Thread.CreatedBy),
    nameof(Thread.CreatedAt), nameof(Thread.Posts))]
public sealed partial class CreateThreadRequestBody;