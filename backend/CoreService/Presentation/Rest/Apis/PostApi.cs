using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        
        api.MapGet(string.Empty, GetPostsAsync);
        
        return app;
    }
    
    private static async Task<Ok<IReadOnlyList<PostDto>>> GetPostsAsync(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] ThreadId? threadId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetPostsQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            ThreadId = threadId
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}