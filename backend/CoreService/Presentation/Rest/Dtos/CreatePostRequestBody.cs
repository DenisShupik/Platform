using CoreService.Domain.Entities;
using SharedKernel.TypeGenerator;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Post), PropertyGenerationMode.AsRequired, nameof(Post.Content))]
public sealed partial class CreatePostRequestBody;