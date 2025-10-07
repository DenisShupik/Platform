using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(AccessPolicyRestrictedError), nameof(AccessPolicyRestrictedError))]
[JsonDerivedType(typeof(CategoryCreatePolicyRestrictedError),nameof(CategoryCreatePolicyRestrictedError))]
[JsonDerivedType(typeof(ThreadCreatePolicyRestrictedError), nameof(ThreadCreatePolicyRestrictedError))]
[JsonDerivedType(typeof(PostCreatePolicyRestrictedError), nameof(PostCreatePolicyRestrictedError))]
public abstract record PolicyRestrictedError(UserId? UserId) : ForbiddenError;

public sealed record AccessPolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);
public sealed record CategoryCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);
public sealed record ThreadCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);
public sealed record PostCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);
