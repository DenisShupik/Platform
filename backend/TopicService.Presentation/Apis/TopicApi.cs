using System.Security.Claims;
using Common.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using TopicService.Application.DTOs;
using TopicService.Domain.Entities;
using TopicService.Infrastructure.Persistence;

namespace TopicService.Presentation.Apis;

public static class TopicApi
{
    public static IEndpointRouteBuilder MapTopicApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/topics")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapPost(string.Empty, CreateTopicAsync);

        // var api = app
        //     .MapGroup("api/threads/{threadId}")
        //     .RequireAuthorization()
        //     .AddFluentValidationAutoValidation();
        // api.MapGet("/posts/count", GetNotesByUserIdCountAsync);
        // api.MapGet("/posts", GetNotesByUserIdAsync);
        // api.MapPost("/posts", CreateNoteAsync);

        return app;
    }

    private static async Task<Ok<long>> CreateTopicAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateTopicRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var topic = new Topic
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Topics.AddAsync(topic, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(topic.TopicId);
    }

    // private static async Task<Results<NotFound, Ok<int>>> GetNotesByUserIdCountAsync(
    //     [AsParameters] GetPostsByThreadIdCountRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //     var query = await dbContext.Posts
    //         .Where(e => e.Topic == request.Topic)
    //         .CountAsync(cancellationToken);
    //
    //     if (query == 0) return TypedResults.NotFound();
    //
    //     return TypedResults.Ok(query);
    // }
    //
    // private static async Task<Ok<KeysetPageResponse<Note>>> GetNotesByUserIdAsync(
    //     [AsParameters] GetPostsByThreadIdRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //
    //     var query = dbContext.Notes
    //         .AsNoTracking()
    //         .OrderBy(e => e.NoteId)
    //         .Where(e => e.UserId == request.UserId);
    //
    //     if (request.Cursor != null)
    //     {
    //         query = query.Where(e => e.NoteId > request.Cursor);
    //     }
    //
    //     var notes = await query.Take(request.PageSize ?? 100).ToListAsync(cancellationToken);
    //     await dbContext.SaveChangesAsync(cancellationToken);
    //     return TypedResults.Ok(new KeysetPageResponse<Note> { Items = notes });
    // }
    //
    // private static async Task<Ok<CreateNoteResponse>> CreateNoteAsync(
    //     [AsParameters] CreateNoteRequest request,
    //     [FromServices] IDbContextFactory<ApplicationDbContext> factory,
    //     CancellationToken cancellationToken
    // )
    // {
    //     var note = new Note
    //     {
    //         UserId = request.UserId,
    //         Title = request.Title
    //     };
    //     await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
    //     await dbContext.Notes.AddAsync(note, cancellationToken);
    //     await dbContext.SaveChangesAsync(cancellationToken);
    //     return TypedResults.Ok(new CreateNoteResponse { NoteId = note.NoteId });
    // }
}