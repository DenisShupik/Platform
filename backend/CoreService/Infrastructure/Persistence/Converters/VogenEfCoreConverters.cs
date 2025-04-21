using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;
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
internal partial class VogenEfCoreConverters;