using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record DuplicateForumModeratorAppointmentError(UserId UserId, ForumId ForumId) : ConflictError;
