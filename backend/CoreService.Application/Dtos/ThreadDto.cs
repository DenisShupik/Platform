using CoreService.Domain.Interfaces;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Dtos;

[Omit(typeof(Thread), PropertyGenerationMode.AsPublic)]
public sealed partial class ThreadDto : IHasCategoryId, IHasThreadId;