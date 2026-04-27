using Shared.Domain.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;

namespace UserService.Domain.Events;

[Include(typeof(User), PropertyGenerationMode.AsRequired, nameof(User.UserId))]
public sealed partial class UserUpdatedDomainEvent : IDomainEvent;