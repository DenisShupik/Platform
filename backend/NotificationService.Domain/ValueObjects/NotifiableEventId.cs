using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace NotificationService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct NotifiableEventId : IId, IHasTryFrom<NotifiableEventId, Guid>
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}