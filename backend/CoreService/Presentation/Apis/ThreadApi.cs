using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;
using Wolverine;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Presentation.Extensions;

namespace CoreService.Presentation.Apis;

public static class ThreadApi
{
    public static IEndpointRouteBuilder MapThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .AddFluentValidationAutoValidation();

        api.MapGet("{threadId}", GetThreadAsync);
        api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync);
        api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync);
        api.MapPost(string.Empty, CreateThreadAsync).RequireAuthorization();
        api.MapPost("{threadId}/posts", CreatePostAsync).RequireAuthorization();
        return app;
    }

    private static async Task<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>>> GetThreadAsync(
        [FromRoute] ThreadId threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadQuery
        {
            ThreadId = threadId
        };

        var result = await messageBus.InvokeAsync<OneOf<ThreadDto, ThreadNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Ok<Dictionary<ThreadId, PostDto>>> GetThreadsPostsLatestAsync(
        [FromRoute] IdList<ThreadId> threadIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsLatestQuery
        {
            ThreadIds = threadIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<ThreadId, PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Dictionary<ThreadId, long>>> GetThreadsPostsCountAsync(
        [FromRoute] IdList<ThreadId> threadIds,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadsPostsCountQuery
        {
            ThreadIds = threadIds
        };

        var result = await messageBus.InvokeAsync<Dictionary<ThreadId, long>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ThreadId>, NotFound<CategoryNotFoundError>>> CreateThreadAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateThreadRequestBody body,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateThreadCommand
        {
            CategoryId = body.CategoryId,
            Title = body.Title,
            UserId = userId
        };

        var result = await messageBus.InvokeAsync<OneOf<ThreadId, CategoryNotFoundError>>(command, cancellationToken);

        return result.Match<Results<Ok<ThreadId>, NotFound<CategoryNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Results<Ok<PostId>, NotFound<ThreadNotFoundError>>> CreatePostAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] ThreadId threadId,
        [FromBody] CreatePostRequestBody body,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreatePostCommand
        {
            ThreadId = threadId,
            Content = body.Content,
            UserId = userId
        };

        var result = await messageBus.InvokeAsync<OneOf<PostId, ThreadNotFoundError>>(command, cancellationToken);

        return result.Match<Results<Ok<PostId>, NotFound<ThreadNotFoundError>>>(
            article => TypedResults.Ok(article),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}