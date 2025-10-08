using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId), nameof(Category.CreatedBy),
    nameof(Category.CreatedAt), nameof(Category.Threads))]
public sealed partial class CreateCategoryRequestBody;