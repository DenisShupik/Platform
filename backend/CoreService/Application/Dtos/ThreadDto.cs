using CoreService.Domain.Interfaces;
using Generator.Attributes;
using SharedKernel.Domain.Interfaces;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Dtos;

[Omit(typeof(Thread), nameof(Thread.Posts))]
public sealed partial class ThreadDto : IHasCategoryId, IHasThreadId, IHasCreateProperties;