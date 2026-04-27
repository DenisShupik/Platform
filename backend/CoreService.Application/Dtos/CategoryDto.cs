using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.Dtos;

[Omit(typeof(Category), PropertyGenerationMode.AsPublic)]
public sealed partial class CategoryDto : IHasCategoryId, IHasForumId;