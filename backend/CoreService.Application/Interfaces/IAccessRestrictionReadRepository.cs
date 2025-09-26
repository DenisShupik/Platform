using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<OneOf<Success, ThreadAccessRestrictedError>> CanUserPostInThreadAsync(UserId userId, ThreadId threadId,
        CancellationToken cancellationToken);
}