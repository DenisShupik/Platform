using System;

namespace SharedKernel.Presentation.Generator.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalAttribute() : Attribute;