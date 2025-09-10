using CoreService.Domain.Entities;
using SharedKernel.TypeGenerator;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.ForumId), nameof(Category.Title))]
public sealed partial class CreateCategoryRequestBody;