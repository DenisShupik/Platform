using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

public sealed class Results<
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult1,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult2,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult3,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult4,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult5,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult6,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult7,
    [DynamicallyAccessedMembers(ResultsOfTHelper.RequireMethods)] TResult8
>
    : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
    where TResult7 : IResult
    where TResult8 : IResult
{
    private Results(IResult activeResult)
    {
        Result = activeResult;
    }

    public IResult Result { get; }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return Result.ExecuteAsync(httpContext);
    }

    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult1 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult2 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult3 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult4 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult5 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult6 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult7 result) => new(result);
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(TResult8 result) => new(result);

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
        ResultsOfTHelper.PopulateMetadataIfTargetIsIEndpointMetadataProvider<TResult8>(method, builder);
    }
}
