using CoreService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Presentation.Rest.Dtos;

[Omit(typeof(ForumPolicySet), PropertyGenerationMode.AsRequired, nameof(ForumPolicySet.ForumPolicySetId),
    nameof(ForumPolicySet.UpdatedBy), nameof(ForumPolicySet.UpdatedAt))]
public sealed partial class CreateForumPolicySetRequestBody;