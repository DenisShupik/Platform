using CoreService.Application.UseCases;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateThreadCommand), PropertyGenerationMode.AsRequired,  nameof(CreateThreadCommand.CreatedBy), nameof(CreateThreadCommand.CreatedAt))]
public sealed partial class CreateThreadRequestBody;