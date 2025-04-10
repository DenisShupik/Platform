using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;
using Vogen;

namespace CoreService.Infrastructure.Persistence.Configurations;

[EfCoreConverter<ForumId>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<ThreadId>]
[EfCoreConverter<PostId>]
[EfCoreConverter<UserId>]
internal partial class VogenEfCoreConverters;