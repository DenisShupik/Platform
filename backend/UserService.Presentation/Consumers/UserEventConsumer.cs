using LinqToDB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Events;
using UserService.Infrastructure.Persistence;
using UserService.Presentation.Extensions;

namespace UserService.Presentation.Consumers;

public sealed class UserEventConsumer : IConsumer<UserEvent>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public UserEventConsumer(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task Consume(ConsumeContext<UserEvent> context)
    {
        var routingKey = context.RoutingKey();

        switch (routingKey)
        {
            case "KK.EVENT.ADMIN.app.SUCCESS.USER.CREATE":
            {
                var typedEvent = context.GetMessage<UserCreatedEvent>();
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var user = new User
                {
                    UserId = typedEvent.UserId,
                    Username = typedEvent.Representation.Username,
                    Email = typedEvent.Representation.Email,
                    Enabled = typedEvent.Representation.Enabled,
                    CreatedAt = typedEvent.CreatedAt,
                };
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
                break;
            case "KK.EVENT.ADMIN.app.SUCCESS.USER.UPDATE":
            {
                var typedEvent = context.GetMessage<UserUpdatedEvent>();
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await dbContext.Users
                    .Where(e => e.UserId == typedEvent.Representation.UserId)
                    .Set(e => e.Username, typedEvent.Representation.Username)
                    .Set(e => e.Email, typedEvent.Representation.Email)
                    .Set(e => e.Enabled, typedEvent.Representation.Enabled)
                    .UpdateAsync();
            }
                break;
            case "KK.EVENT.ADMIN.app.SUCCESS.USER.DELETE":
            {
                var typedEvent = context.GetMessage<UserDeletedEvent>();
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                await dbContext.Users.Where(e => e.UserId == typedEvent.UserId).DeleteAsync();
                await dbContext.SaveChangesAsync();
            }
                break;
            case "KK.EVENT.CLIENT.app.SUCCESS.app-user.REGISTER":
            {
                var typedEvent = context.GetMessage<UserRegisteredEvent>();
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var user = new User
                {
                    UserId = typedEvent.UserId,
                    Username = typedEvent.Details.Username,
                    Email = typedEvent.Details.Email,
                    Enabled = true,
                    CreatedAt = typedEvent.RegisteredAt
                };
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
                break;
            default:
                return;
        }
    }
}