using CoreService.Domain.ValueObjects;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using Vogen;

namespace NotificationService.Infrastructure.Persistence.Converters;

[EfCoreConverter<ThreadId>]
[EfCoreConverter<PostId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<NotifiableEventId>]
internal partial class VogenEfCoreConverters;