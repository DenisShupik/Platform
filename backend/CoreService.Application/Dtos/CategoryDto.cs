using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Generator.Attributes;
using UserService.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Category), PropertyGenerationMode.AsPublic, nameof(Category.Threads))]
public sealed partial class CategoryDto : IHasCategoryId, IHasForumId, IHasCreateProperties;