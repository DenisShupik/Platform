using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using Vogen;

namespace NotificationService.Infrastructure.Persistence.Converters;

[EfCoreConverter<ThreadId>]
[EfCoreConverter<UserId>]
internal partial class VogenEfCoreConverters;