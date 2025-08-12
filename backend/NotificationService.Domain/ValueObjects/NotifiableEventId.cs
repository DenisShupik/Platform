using SharedKernel.Domain.Helpers;
using SharedKernel.Domain.Interfaces;
using Vogen;

namespace NotificationService.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson)]
public readonly partial struct NotifiableEventId : IId, IVogen<NotifiableEventId, Guid>
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}