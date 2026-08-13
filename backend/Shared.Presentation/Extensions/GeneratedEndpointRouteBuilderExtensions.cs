using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shared.Presentation.Abstractions;

namespace Shared.Presentation.Extensions;

public static class GeneratedEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Maps a generated GET endpoint for <typeparamref name="TRequest"/> to
        /// <typeparamref name="THandler"/>.
        /// </summary>
        public RouteHandlerBuilder MapGet<TRequest, THandler>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class =>
            TRequest.Map(endpoints, pattern);

        /// <summary>
        /// Maps a generated POST endpoint for <typeparamref name="TRequest"/> to
        /// <typeparamref name="THandler"/>. For conventional create requests, the generator
        /// infers the target GET endpoint used by the Location header.
        /// </summary>
        public RouteHandlerBuilder MapPost<TRequest, THandler>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class =>
            TRequest.Map(endpoints, pattern);

        /// <summary>
        /// Maps a generated POST endpoint and explicitly selects the request type of the GET
        /// endpoint used to produce the Location header.
        /// </summary>
        public RouteHandlerBuilder MapPostCreatedAt<TRequest, THandler, TCreatedAtRequest>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class
            where TCreatedAtRequest : class =>
            TRequest.Map(endpoints, pattern);

        /// <summary>
        /// Maps a generated PUT endpoint for <typeparamref name="TRequest"/> to
        /// <typeparamref name="THandler"/>.
        /// </summary>
        public RouteHandlerBuilder MapPut<TRequest, THandler>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class =>
            TRequest.Map(endpoints, pattern);

        /// <summary>
        /// Maps a generated PATCH endpoint for <typeparamref name="TRequest"/> to
        /// <typeparamref name="THandler"/>.
        /// </summary>
        public RouteHandlerBuilder MapPatch<TRequest, THandler>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class =>
            TRequest.Map(endpoints, pattern);

        /// <summary>
        /// Maps a generated DELETE endpoint for <typeparamref name="TRequest"/> to
        /// <typeparamref name="THandler"/>.
        /// </summary>
        public RouteHandlerBuilder MapDelete<TRequest, THandler>(
            [StringSyntax("Route")] string pattern)
            where TRequest : class, IGeneratedEndpoint<TRequest, THandler>
            where THandler : class =>
            TRequest.Map(endpoints, pattern);
    }
}
