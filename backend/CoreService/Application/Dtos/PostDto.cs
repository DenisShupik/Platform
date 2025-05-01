using CoreService.Domain.Entities;
using CoreService.Domain.Interfaces;
using Generator.Attributes;
using SharedKernel.Domain.Interfaces;

namespace CoreService.Application.Dtos;

[Omit(typeof(Post))]
public sealed partial class PostDto : IHasThreadId, IHasCreateProperties,IHasUpdateProperties;