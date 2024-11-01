using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Presentation.Apis;

public static class UserApi
{
    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/users")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("{userId}", GetUserAsync);
        return app;
    }

    private static async Task<Results<NotFound, Ok<User>>> GetUserAsync(
        [AsParameters] GetUserRequest request,
        [FromServices] IDbContextFactory<ApplicationDbContext> factory,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await factory.CreateDbContextAsync(cancellationToken);
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == request.UserId, cancellationToken);
        if (user == null) return TypedResults.NotFound();
        return TypedResults.Ok(user);
    }
}