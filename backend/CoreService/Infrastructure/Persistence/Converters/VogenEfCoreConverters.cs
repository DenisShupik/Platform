using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using Vogen;

namespace CoreService.Infrastructure.Persistence.Converters;

[EfCoreConverter<ForumId>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<ThreadId>]
[EfCoreConverter<PostId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<ForumTitle>]
[EfCoreConverter<CategoryTitle>]
[EfCoreConverter<ThreadTitle>]
[EfCoreConverter<PostContent>]
[EfCoreConverter<PostIndex>]
internal partial class VogenEfCoreConverters;