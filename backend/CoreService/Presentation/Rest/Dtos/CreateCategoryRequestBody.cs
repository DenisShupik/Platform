using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(CreateCategoryCommand), PropertyGenerationMode.AsRequired, nameof(CreateCategoryCommand.CreatedBy),
    nameof(CreateCategoryCommand.CreatedAt))]
public sealed partial class CreateCategoryRequestBody;