using Generator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Presentation.Rest.Dtos;

[IncludeAsRequired(typeof(Thread), nameof(Thread.CategoryId),nameof(Thread.Title))]
public sealed partial class CreateThreadRequestBody;