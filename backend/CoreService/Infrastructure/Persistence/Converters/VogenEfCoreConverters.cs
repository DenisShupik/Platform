using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;
using Index = Shared.Domain.ValueObjects.Index;
using Vogen;

namespace CoreService.Infrastructure.Persistence.Converters;

[EfCoreConverter<ForumId>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<ThreadId>]
[EfCoreConverter<PostId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<RoleId>]
[EfCoreConverter<RoleTitle>]
[EfCoreConverter<ForumTitle>]
[EfCoreConverter<CategoryTitle>]
[EfCoreConverter<ThreadTitle>]
[EfCoreConverter<PostContent>]
[EfCoreConverter<Count>]
[EfCoreConverter<Index>]
internal partial class VogenEfCoreConverters;