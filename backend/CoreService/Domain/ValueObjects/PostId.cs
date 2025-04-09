using SharedKernel.Domain.Helpers;
using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<long>(conversions: Conversions.SystemTextJson)]
public readonly partial struct PostId : IVogen<PostId, long>
{
    private static Validation Validate(in long value) => ValidationHelper.LongValidate(value);
}