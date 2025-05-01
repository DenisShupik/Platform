using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Apis.Dtos;

[IncludeAsRequired(typeof(Post), nameof(Post.Content))]
public sealed partial class CreatePostRequestBody;