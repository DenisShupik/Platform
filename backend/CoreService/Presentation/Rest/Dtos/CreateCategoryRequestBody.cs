using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[IncludeAsRequired(typeof(Category), nameof(Category.ForumId), nameof(Category.Title))]
public sealed partial class CreateCategoryRequestBody;