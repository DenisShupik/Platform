using System.Data.Common;
using LinqToDB.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shared.Infrastructure.Diagnostics;

internal static class RepositoryCallCommandTagger
{
    internal static DbCommand AddRepositoryCall(DbCommand command, RepositoryCallContextAccessor contextAccessor)
    {
        var repositoryCall = contextAccessor.Current;
        if (repositoryCall is null)
        {
            return command;
        }

        command.CommandText = $"/* {repositoryCall.Replace("*/", "* /", StringComparison.Ordinal)} */{Environment.NewLine}{command.CommandText}";
        return command;
    }
}

internal sealed class EfRepositoryCallCommandInterceptor(RepositoryCallContextAccessor contextAccessor)
    : DbCommandInterceptor
{
    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result) =>
        RepositoryCallCommandTagger.AddRepositoryCall(result, contextAccessor);
}

internal sealed class LinqToDbRepositoryCallCommandInterceptor(RepositoryCallContextAccessor contextAccessor)
    : CommandInterceptor
{
    public override DbCommand CommandInitialized(LinqToDB.Interceptors.CommandEventData eventData, DbCommand command) =>
        RepositoryCallCommandTagger.AddRepositoryCall(command, contextAccessor);
}
