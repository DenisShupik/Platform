using System.Security.Claims;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Errors;
using NotificationService.Presentation.Apis.Dtos;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Apis;

public static class SubscriptionApi
{
    public static IEndpointRouteBuilder MapSubscriptionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/thread/{threadId}/subscriptions")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/status", GetThreadSubscriptionStatusAsync);
        api.MapPost(string.Empty, CreateThreadSubscriptionAsync);
        api.MapDelete(string.Empty, DeleteThreadSubscriptionAsync);

        return app;
    }

    private static async Task<Ok<GetThreadSubscriptionStatusQueryResult>> GetThreadSubscriptionStatusAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] ThreadId threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetThreadSubscriptionStatusQuery
        {
            UserId = userId,
            ThreadId = threadId,
        };
        var result = await messageBus.InvokeAsync<GetThreadSubscriptionStatusQueryResult>(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, Conflict<DuplicateThreadSubscriptionError>>>
        CreateThreadSubscriptionAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromBody] CreateThreadSubscriptionRequestBody body,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateThreadSubscriptionCommand
        {
            UserId = userId,
            ThreadId = threadId,
            Channels = body.Channels
        };
        var result = await messageBus.InvokeAsync<CreateThreadSubscriptionResult>(command, cancellationToken);

        return result
            .Match<Results<NoContent, Conflict<DuplicateThreadSubscriptionError>>>(
                _ => TypedResults.NoContent(),
                duplicateError => TypedResults.Conflict(duplicateError)
            );
    }

    private static async Task<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>
        DeleteThreadSubscriptionAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new DeleteThreadSubscriptionCommand
        {
            UserId = userId,
            ThreadId = threadId,
        };
        var result = await messageBus.InvokeAsync<DeleteThreadSubscriptionCommandResult>(command, cancellationToken);

        return result
            .Match<Results<NoContent, NotFound<ThreadSubscriptionNotFoundError>>>(
                _ => TypedResults.NoContent(),
                notFoundError => TypedResults.NotFound(notFoundError)
            );
    }
}