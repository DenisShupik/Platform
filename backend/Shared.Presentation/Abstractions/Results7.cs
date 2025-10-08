using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

public sealed class Results<
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult1,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult2,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult3,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult4,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult5,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult6,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)]
    TResult7
>
    : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
    where TResult7 : IResult
{
    // Use implicit cast operators to create an instance
    private Results(IResult activeResult)
    {
        Result = activeResult;
    }

    /// <summary>
    /// Gets the actual <see cref="IResult"/> returned by the <see cref="Endpoint"/> route handler delegate.
    /// </summary>
    public IResult Result { get; }

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (Result is null)
        {
            throw new InvalidOperationException("The IResult assigned to the Result property must not be null.");
        }

        return Result.ExecuteAsync(httpContext);
    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult1 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult2 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult3"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult3 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult4"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult4 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult5"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult5 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult6"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult6 result) =>
        new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult7"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}" />.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator
        Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(TResult7 result) =>
        new(result);

    /// <inheritdoc/>
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult1>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult2>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult3>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult4>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult5>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult6>(method, builder);
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult7>(method, builder);
    }
}