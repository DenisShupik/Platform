using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static class PostApi
{
    public static IEndpointRouteBuilder MapPostApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/posts")
            .AddFluentValidationAutoValidation();

        api.MapGet("{postId}/order", GetPostIndexAsync);
        api.MapPatch("{postId}", UpdatePostAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>> GetPostIndexAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] PostId postId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var command = new GetPostIndexQuery
        {
            PostId = postId
        };

        var result = await messageBus.InvokeAsync<GetPostIndexQueryResult>(command, cancellationToken);

        return result.Match<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>>(
            order => TypedResults.Ok(order),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async
        Task<Results<Ok, NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>>>
        UpdatePostAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] PostId postId,
            [FromBody] UpdatePostRequestBody body,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new UpdatePostCommand
        {
            PostId = postId,
            Content = body.Content,
            RowVersion = body.RowVersion,
            UpdateBy = userId
        };

        var result = await messageBus.InvokeAsync<UpdatePostCommandResult>(command, cancellationToken);

        return result
            .Match<Results<Ok, NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>>>(
                _ => TypedResults.Ok(),
                notFound => TypedResults.NotFound(notFound),
                nonPostAuthorError => new Forbid<NonPostAuthorError>(nonPostAuthorError),
                staleError => TypedResults.Conflict(staleError)
            );
    }
}