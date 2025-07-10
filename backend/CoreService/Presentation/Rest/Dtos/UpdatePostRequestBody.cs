using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[IncludeAsRequired(typeof(Post), nameof(Post.Content), nameof(Post.RowVersion))]
public sealed partial class UpdatePostRequestBody;