using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Shared.Infrastructure.Diagnostics;

internal class RepositoryCallProxy<TRepository> : DispatchProxy
    where TRepository : class
{
    private static readonly string RepositoryName = GetRepositoryName();
    private RepositoryCallContextAccessor _contextAccessor = null!;
    private TRepository _target = null!;

    public static TRepository Create(TRepository target, RepositoryCallContextAccessor contextAccessor)
    {
        var repository = Create<TRepository, RepositoryCallProxy<TRepository>>();
        var proxy = (RepositoryCallProxy<TRepository>)(object)repository;
        proxy._target = target;
        proxy._contextAccessor = contextAccessor;
        return repository;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        using var repositoryCall = _contextAccessor.Push($"{RepositoryName}.{targetMethod.Name}");

        try
        {
            return targetMethod.Invoke(_target, args);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static string GetRepositoryName()
    {
        var name = typeof(TRepository).Name;
        return name.Length > 1 && name[0] == 'I' ? name[1..] : name;
    }
}
