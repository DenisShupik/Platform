using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using SharedKernel.TypeGenerator;
using UserService.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Forum), PropertyGenerationMode.AsPublic, nameof(Forum.Categories))]
public sealed partial class ForumDto : IHasForumId, IHasCreateProperties;