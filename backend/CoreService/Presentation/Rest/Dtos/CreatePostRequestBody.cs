using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[IncludeAsRequired(typeof(Post), nameof(Post.Content))]
public sealed partial class CreatePostRequestBody;