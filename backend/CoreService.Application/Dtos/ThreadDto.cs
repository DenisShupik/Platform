using CoreService.Domain.Interfaces;
using Generator.Attributes;
using UserService.Domain.Interfaces;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Dtos;

[Omit(typeof(Thread), PropertyGenerationMode.AsPublic, nameof(Thread.Posts))]
public sealed partial class ThreadDto : IHasCategoryId, IHasThreadId, IHasCreateProperties;