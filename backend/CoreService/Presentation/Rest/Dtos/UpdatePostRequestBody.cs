using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.Content), nameof(Post.RowVersion))]
public sealed partial class UpdatePostRequestBody;