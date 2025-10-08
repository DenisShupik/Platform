using CoreService.Application.UseCases;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateForumCommand), PropertyGenerationMode.AsRequired, nameof(CreateForumCommand.CreatedBy),
    nameof(CreateForumCommand.CreatedAt))]
public sealed partial class CreateForumRequestBody;