using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(Forum), PropertyGenerationMode.AsRequired, nameof(Forum.ForumId), nameof(Forum.CreatedBy),
    nameof(Forum.CreatedAt), nameof(Forum.Categories))]
public sealed partial class CreateForumRequestBody;