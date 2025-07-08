using CoreService.Domain.ValueObjects;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using Vogen;

namespace NotificationService.Infrastructure.Persistence.Converters;

[EfCoreConverter<ThreadId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<NotificationId>]
internal partial class VogenEfCoreConverters;