using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Shared.Infrastructure.Diagnostics;

internal class RepositoryCallProxy<TRepository> : DispatchProxy
    where TRepository : class
{
    private static readonly string RepositoryName = GetRepositoryName();
    private static readonly MethodInfo InvokeTaskOfTMethod = GetGenericWrapperMethod(nameof(InvokeTaskAsync));
    private static readonly MethodInfo InvokeValueTaskOfTMethod = GetGenericWrapperMethod(nameof(InvokeValueTaskAsync));
    private static readonly ConcurrentDictionary<Type, MethodInfo> TaskWrappers = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> ValueTaskWrappers = new();
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

        var returnType = targetMethod.ReturnType;
        if (returnType == typeof(Task))
        {
            return InvokeTaskAsync(targetMethod, args);
        }

        if (returnType == typeof(ValueTask))
        {
            return InvokeValueTaskAsync(targetMethod, args);
        }

        if (returnType.IsGenericType)
        {
            var genericTypeDefinition = returnType.GetGenericTypeDefinition();
            var resultType = returnType.GenericTypeArguments[0];

            if (genericTypeDefinition == typeof(Task<>))
            {
                return TaskWrappers
                    .GetOrAdd(resultType, static type => InvokeTaskOfTMethod.MakeGenericMethod(type))
                    .Invoke(this, [targetMethod, args]);
            }

            if (genericTypeDefinition == typeof(ValueTask<>))
            {
                return ValueTaskWrappers
                    .GetOrAdd(resultType, static type => InvokeValueTaskOfTMethod.MakeGenericMethod(type))
                    .Invoke(this, [targetMethod, args]);
            }
        }

        using var repositoryCall = PushRepositoryCall(targetMethod);
        return InvokeTarget(targetMethod, args);
    }

    private async Task InvokeTaskAsync(MethodInfo targetMethod, object?[]? args)
    {
        using var repositoryCall = PushRepositoryCall(targetMethod);
        var task = (Task?)InvokeTarget(targetMethod, args)
                   ?? throw new InvalidOperationException($"{targetMethod.Name} returned null instead of Task.");
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InvokeTaskAsync<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        using var repositoryCall = PushRepositoryCall(targetMethod);
        var task = (Task<TResult>?)InvokeTarget(targetMethod, args)
                   ?? throw new InvalidOperationException($"{targetMethod.Name} returned null instead of Task.");
        return await task.ConfigureAwait(false);
    }

    private async ValueTask InvokeValueTaskAsync(MethodInfo targetMethod, object?[]? args)
    {
        using var repositoryCall = PushRepositoryCall(targetMethod);
        var valueTask = (ValueTask?)InvokeTarget(targetMethod, args)
                        ?? throw new InvalidOperationException($"{targetMethod.Name} returned null instead of ValueTask.");
        await valueTask.ConfigureAwait(false);
    }

    private async ValueTask<TResult> InvokeValueTaskAsync<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        using var repositoryCall = PushRepositoryCall(targetMethod);
        var valueTask = (ValueTask<TResult>?)InvokeTarget(targetMethod, args)
                        ?? throw new InvalidOperationException($"{targetMethod.Name} returned null instead of ValueTask.");
        return await valueTask.ConfigureAwait(false);
    }

    private IDisposable PushRepositoryCall(MethodInfo targetMethod) =>
        _contextAccessor.Push($"{RepositoryName}.{targetMethod.Name}");

    private object? InvokeTarget(MethodInfo targetMethod, object?[]? args)
    {
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

    private static MethodInfo GetGenericWrapperMethod(string name) =>
        typeof(RepositoryCallProxy<TRepository>)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == name && method.IsGenericMethodDefinition);

    private static string GetRepositoryName()
    {
        var name = typeof(TRepository).Name;
        return name.Length > 1 && name[0] == 'I' ? name[1..] : name;
    }
}
