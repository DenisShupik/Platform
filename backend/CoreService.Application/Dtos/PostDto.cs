using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using SharedKernel.TypeGenerator;
using UserService.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Post), PropertyGenerationMode.AsPublic)]
public sealed partial class PostDto : IHasThreadId, IHasCreateProperties, IHasUpdateProperties;