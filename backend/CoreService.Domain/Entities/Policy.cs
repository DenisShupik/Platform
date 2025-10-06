using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Политика
/// </summary>
public sealed class Policy
{
    /// <summary>
    /// Идентификатор политики
    /// </summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>
    /// Тип политики
    /// </summary>
    public PolicyType Type { get; private set; }

    /// <summary>
    /// Значение политики
    /// </summary>
    public PolicyValue Value { get; private set; }

    public Policy(PolicyType type, PolicyValue value)
    {
        PolicyId = PolicyId.From(Guid.CreateVersion7());
        Type = type;
        Value = value;
    }
}