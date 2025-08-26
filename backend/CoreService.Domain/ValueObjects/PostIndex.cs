using Vogen;

namespace CoreService.Domain.ValueObjects;

[ValueObject<ulong>(conversions: Conversions.SystemTextJson)]
public readonly partial struct PostIndex;