using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Generator.Attributes;
using SharedKernel.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Forum),nameof(Forum.Categories))]
public sealed partial class ForumDto : IHasForumId, IHasCreateProperties;