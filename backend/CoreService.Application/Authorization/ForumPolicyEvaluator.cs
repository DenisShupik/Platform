using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Authorization;

public sealed class ForumPolicyEvaluator(ICapabilityGrantRepository grants) : IForumPolicyEvaluator
{
    public async Task<IReadOnlySet<ForumPolicy>> GetAllowedAsync(
        ActorContext actor,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        var capabilities = await grants.GetActiveCapabilitiesAsync(
            actor.UserId,
            scope,
            evaluatedAt,
            cancellationToken);

        var allowed = new HashSet<ForumPolicy>();
        foreach (var policy in Enum.GetValues<ForumPolicy>())
        {
            if (capabilities.Contains(ToCapability(policy))) allowed.Add(policy);
        }

        return allowed;
    }

    public async Task<SuccessOr<PermissionDeniedError>> AuthorizeAsync(
        ActorContext actor,
        ForumPolicy policy,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        var capability = ToCapability(policy);

        return await grants.HasActiveCapabilityAsync(
            actor.UserId,
            capability,
            scope,
            evaluatedAt,
            cancellationToken)
            ? SuccessOr.Success
            : new PermissionDeniedError();
    }

    private static CapabilityCode ToCapability(ForumPolicy policy) => policy switch
    {
        ForumPolicy.ManageStructure => CapabilityCode.ManageStructure,
        ForumPolicy.ViewUnpublishedThreads => CapabilityCode.ViewUnpublishedThreads,
        ForumPolicy.ApproveThread => CapabilityCode.ApproveThreads,
        ForumPolicy.RejectThread => CapabilityCode.RejectThreads,
        ForumPolicy.EditAnyPost => CapabilityCode.EditAnyPost,
        ForumPolicy.DeleteAnyPost => CapabilityCode.DeleteAnyPost,
        ForumPolicy.ManageAuthorization => CapabilityCode.ManageAuthorization,
        ForumPolicy.ManageSanctions => CapabilityCode.ManageSanctions,
        _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
    };
}
