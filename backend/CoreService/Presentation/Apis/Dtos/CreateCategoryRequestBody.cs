using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;

namespace CoreService.Presentation.Apis.Dtos;

[IncludeAsRequired(typeof(Category), nameof(Category.ForumId), nameof(Category.Title))]
public sealed partial class CreateCategoryRequestBody;