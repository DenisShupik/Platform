using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static class ThreadApi
{
    public static IEndpointRouteBuilder MapThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetThreadsPagedAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("count", GetThreadsCountAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{threadId}", GetThreadAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{threadId}/posts", GetThreadPostsPagedAsync);
        api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync);
        api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync);
        api.MapPost(string.Empty, CreateThreadAsync).RequireAuthorization();
        api.MapPost("{threadId}/posts", CreatePostAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsPagedAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromQuery] PaginationOffset? offset,
            [FromQuery] PaginationLimitMin10Max100Default100? limit,
            [FromQuery] SortCriteriaList<GetThreadsPagedQuery.GetThreadsPagedQuerySortType>? sort,
            [FromQuery] UserId? createdBy,
            [FromQuery] ThreadStatus? status,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsPagedQuery
        {
            Offset = offset,
            Limit = limit,
            CreatedBy = createdBy,
            Status = status,
            QueriedBy = userId,
            Sort = sort
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
            Role = RoleType.User,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadQueryResult<ThreadDto>>(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            threadDto => TypedResults.Ok(threadDto),
            notFound => TypedResults.NotFound(notFound),
            nonThreadOwner => new Forbid<NonThreadOwnerError>(nonThreadOwner)
        );
    }

    private static async Task<Ok<IReadOnlyList<PostDto>>> GetThreadPostsPagedAsync(
        [FromQuery] PaginationOffset? offset,
        [FromQuery] PaginationLimitMin10Max100Default100? limit,
        [FromQuery] SortCriteriaList<GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType>? sort,
        [FromRoute] ThreadId threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadPostsPagedQuery
        {
            Offset = offset,
            Limit = limit,
            ThreadId = threadId,
            Sort = sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Dictionary<ThreadId, PostDto>>> GetThreadsPostsLatestAsync(
        [FromRoute] IdSet<ThreadId> threadIds,
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
        [FromRoute] IdSet<ThreadId> threadIds,
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
            CreatedBy = userId
        };

        var result = await messageBus.InvokeAsync<CreateThreadCommandResult>(command, cancellationToken);

        return result.Match<Results<Ok<ThreadId>, NotFound<CategoryNotFoundError>>>(
            threadId => TypedResults.Ok(threadId),
            notFound => TypedResults.NotFound(notFound)
        );
    }

    private static async Task<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>
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
            CreatedBy = userId
        };

        var result = await messageBus.InvokeAsync<CreatePostCommandResult>(command, cancellationToken);

        return result.Match<Results<Ok<PostId>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            postId => TypedResults.Ok(postId),
            notFound => TypedResults.NotFound(notFound),
            nonThreadAuthor => new Forbid<NonThreadOwnerError>(nonThreadAuthor)
        );
    }
}