using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.Dtos;

[Omit(typeof(Policy), PropertyGenerationMode.AsPublic, nameof(Policy.AddedPolicies))]
public sealed partial class PolicyDto;