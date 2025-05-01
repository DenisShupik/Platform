using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Apis.Dtos;

[IncludeAsRequired(typeof(Post), nameof(Post.Content), nameof(Post.RowVersion))]
public sealed partial class UpdatePostRequestBody;