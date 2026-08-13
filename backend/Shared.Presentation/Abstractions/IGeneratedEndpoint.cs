using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Shared.Presentation.Abstractions;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGeneratedEndpoint<TRequest, THandler>
    where TRequest : class
    where THandler : class
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract RouteHandlerBuilder Map(
        IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern);
}
