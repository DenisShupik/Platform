using Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteService.Application.DTOs;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Persistence;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace NoteService.Presentation.Apis;

public static class NoteApi
{
    public static IEndpointRouteBuilder MapNoteApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/users/{userId}")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/notes/count", GetNotesByUserIdCountAsync);
        api.MapGet("/notes", GetNotesByUserIdAsync);
        api.MapPost("/notes", CreateNoteAsync);

        return app;
    }

    private static async Task<Results<NotFound, Ok<int>>> GetNotesByUserIdCountAsync(
        [AsParameters] GetNotesByUserIdCountRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var query = await dbContext.Notes
            .Where(e => e.UserId == request.UserId)
            .CountAsync(cancellationToken);

        if (query == 0) return TypedResults.NotFound();

        return TypedResults.Ok(query);
    }

    private static async Task<Ok<KeysetPageResponse<Note>>> GetNotesByUserIdAsync(
        [AsParameters] GetNotesByUserIdRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Notes
            .AsNoTracking()
            .OrderBy(e => e.NoteId)
            .Where(e => e.UserId == request.UserId);

        if (request.Cursor != null)
        {
            query = query.Where(e => e.NoteId > request.Cursor);
        }

        var notes = await query.Take(request.PageSize ?? 100).ToListAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new KeysetPageResponse<Note> { Items = notes });
    }

    private static async Task<Ok<CreateNoteResponse>> CreateNoteAsync(
        [AsParameters] CreateNoteRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        var note = new Note
        {
            UserId = request.UserId,
            Title = request.Title
        };
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        await dbContext.Notes.AddAsync(note, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new CreateNoteResponse { NoteId = note.NoteId });
    }
}