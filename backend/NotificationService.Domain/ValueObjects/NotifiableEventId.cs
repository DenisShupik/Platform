using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace NotificationService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct NotifiableEventId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}