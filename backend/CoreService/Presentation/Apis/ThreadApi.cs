using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Apis.Dtos;
using Microsoft.OpenApi.Models;
using Wolverine;
using OneOf;
using SharedKernel.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Presentation.Extensions;

namespace CoreService.Presentation.Apis;

public static class ThreadApi
{
    public static IEndpointRouteBuilder MapThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetThreadsAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("count", GetThreadsCountAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{threadId}", GetThreadAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync);
        api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync);
        api.MapPut("{threadId}/posts/{postId}/order", GetPostOrderAsync);
        api.MapPost(string.Empty, CreateThreadAsync).RequireAuthorization();
        api.MapPost("{threadId}/posts", CreatePostAsync).RequireAuthorization();
        api.MapPut("{threadId}/posts/{postId}", UpdatePostAsync).RequireAuthorization();
        return app;
    }

    private static async Task<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromQuery] int? offset,
            [FromQuery] int? limit,
            [FromQuery] UserId? createdBy,
            [FromQuery] ThreadStatus? status,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            CreatedBy = createdBy,
            Status = status,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadsQueryResult<ThreadDto>>(query, cancellationToken);

        return result.Match<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            threads => TypedResults.Ok(threads),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }

    private static async Task<Results<Ok<long>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromQuery] UserId? createdBy,
            [FromQuery] ThreadStatus? status,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsCountQuery
        {
            CreatedBy = createdBy,
            Status = status,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadsCountQueryResult>(query, cancellationToken);

        return result.Match<Results<Ok<long>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            count => TypedResults.Ok(count),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }

    private static async Task<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>
        GetThreadAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadQuery
        {
            ThreadId = threadId,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadQueryResult<ThreadDto>>(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            threadDto => TypedResults.Ok(threadDto),
            notFound => TypedResults.NotFound(notFound),
            nonThreadOwner => new Forbid<NonThreadOwnerError>(nonThreadOwner)
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

    private static async Task<Results<NotFound<CategoryNotFoundError>, Ok<ThreadId>>> CreateThreadAsync(
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

        var result = await messageBus.InvokeAsync<CreateThreadCommandResult>(command, cancellationToken);

        return result.Match<Results<NotFound<CategoryNotFoundError>, Ok<ThreadId>>>(
            notFound => TypedResults.NotFound(notFound),
            threadId => TypedResults.Ok(threadId)
        );
    }

    private static async Task<Results<Ok<long>, NotFound<PostNotFoundError>>> GetPostOrderAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] ThreadId threadId,
        [FromRoute] PostId postId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var command = new GetPostOrderQuery
        {
            ThreadId = threadId,
            PostId = postId
        };

        var result = await messageBus.InvokeAsync<GetPostOrderQueryResult>(command, cancellationToken);

        return result.Match<Results<Ok<long>, NotFound<PostNotFoundError>>>(
            order => TypedResults.Ok(order),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Results<NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>, Ok<PostId>>>
        CreatePostAsync(
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

        var result = await messageBus.InvokeAsync<CreatePostCommandResult>(command, cancellationToken);

        return result.Match<Results<NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>, Ok<PostId>>>(
            notFound => TypedResults.NotFound(notFound),
            nonThreadAuthor => new Forbid<NonThreadOwnerError>(nonThreadAuthor),
            postId => TypedResults.Ok(postId)
        );
    }

    private static async
        Task<Results<NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>, Ok>>
        UpdatePostAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromRoute] PostId postId,
            [FromBody] UpdatePostRequestBody body,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new UpdatePostCommand
        {
            ThreadId = threadId,
            PostId = postId,
            Content = body.Content,
            RowVersion = body.RowVersion,
            UpdateBy = userId
        };

        var result = await messageBus.InvokeAsync<UpdatePostCommandResult>(command, cancellationToken);

        return result
            .Match<Results<NotFound<PostNotFoundError>, Forbid<NonPostAuthorError>, Conflict<PostStaleError>, Ok>>(
                notFound => TypedResults.NotFound(notFound),
                nonPostAuthorError => new Forbid<NonPostAuthorError>(nonPostAuthorError),
                staleError => TypedResults.Conflict(staleError),
                _ => TypedResults.Ok()
            );
    }
}