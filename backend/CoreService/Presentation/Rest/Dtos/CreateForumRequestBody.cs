using CoreService.Domain.Entities;
using Generator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Include(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.Title))]
public sealed partial class CreateForumRequestBody;