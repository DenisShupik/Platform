using System.Data.Common;
using LinqToDB.Interceptors;

namespace Shared.Infrastructure.Diagnostics;

internal sealed class LinqToDbRepositoryCallCommandInterceptor(RepositoryCallContextAccessor contextAccessor)
    : CommandInterceptor
{
    public override DbCommand CommandInitialized(CommandEventData eventData, DbCommand command) =>
        RepositoryCallCommandTagger.AddRepositoryCall(command, contextAccessor);
}
