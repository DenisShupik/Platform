using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Apis.Dtos;

[IncludeAsRequired(typeof(Forum), nameof(Forum.Title))]
public sealed partial class CreateForumRequestBody;