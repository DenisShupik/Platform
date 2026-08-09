using System.Data.Common;

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

        var tag = $"/* {repositoryCall.Replace("*/", "* /", StringComparison.Ordinal)} */";
        if (!command.CommandText.StartsWith(tag, StringComparison.Ordinal))
        {
            command.CommandText = $"{tag}{Environment.NewLine}{command.CommandText}";
        }

        return command;
    }
}
