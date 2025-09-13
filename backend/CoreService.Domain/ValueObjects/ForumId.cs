using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct ForumId : IId
{
    private static Validation Validate(in Guid value) => ValidationHelper.GuidValidate(value);
}