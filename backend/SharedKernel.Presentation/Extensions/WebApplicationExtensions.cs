using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.Interfaces;

namespace SharedKernel.Presentation.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrations<TWritableDbContext>(this WebApplication app)
        where TWritableDbContext : DbContext, IWritableDbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWritableDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}