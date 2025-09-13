using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title))]
public sealed partial class CreateForumRequestBody;