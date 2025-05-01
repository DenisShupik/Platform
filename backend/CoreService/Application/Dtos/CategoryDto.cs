using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Generator.Attributes;
using SharedKernel.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Category),nameof(Category.Threads))]
public sealed partial class CategoryDto : IHasCategoryId, IHasForumId, IHasCreateProperties;