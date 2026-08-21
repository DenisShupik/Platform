using CoreService.Domain.Enums;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record DuplicateCapabilityGrantError(UserId UserId, CapabilityCode Capability) : ConflictError;
