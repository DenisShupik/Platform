using Shared.TypeGenerator.Attributes;
using Shared.Domain.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Domain.DomainEvents;

[Include(typeof(User), PropertyGenerationMode.AsRequired, nameof(User.UserId))]
public sealed partial class UserUpdatedDomainEvent : IDomainEvent;