using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record ForumModeratorAppointmentNotFoundError(UserId UserId, ForumId ForumId) : NotFoundError;
