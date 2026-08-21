using Shared.Domain.Abstractions.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record PlatformAdministratorAppointmentNotFoundError(UserId UserId) : NotFoundError;
