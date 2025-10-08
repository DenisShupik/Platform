using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Post), PropertyGenerationMode.AsPublic)]
public sealed partial class PostDto : IHasThreadId;