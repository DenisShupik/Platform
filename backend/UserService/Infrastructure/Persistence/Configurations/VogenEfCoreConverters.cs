using UserService.Domain.ValueObjects;
using Vogen;

namespace UserService.Infrastructure.Persistence.Configurations;

[EfCoreConverter<UserId>]
[EfCoreConverter<Username>]
internal partial class VogenEfCoreConverters;