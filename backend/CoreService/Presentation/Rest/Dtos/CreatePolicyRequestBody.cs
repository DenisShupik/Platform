using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(Policy), PropertyGenerationMode.AsRequired, nameof(Policy.PolicyId))]
public sealed partial class CreatePolicyRequestBody;