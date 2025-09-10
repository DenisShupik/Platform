using CoreService.Domain.Entities;
using SharedKernel.TypeGenerator;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title))]
public sealed partial class CreateForumRequestBody;