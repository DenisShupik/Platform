using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.Dtos;

[Omit(typeof(Category), PropertyGenerationMode.AsPublic, nameof(Category.Threads))]
public sealed partial class CategoryDto : IHasCategoryId, IHasForumId;