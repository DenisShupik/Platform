using LinqToDB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Options;
using UserService.Domain.Entities;
using UserService.Infrastructure.Events;
using UserService.Infrastructure.Extensions;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Consumers;

public sealed class UserEventConsumer : IConsumer<UserEvent>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _client;
    private readonly string _realm;

    public UserEventConsumer(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IOptions<KeycloakOptions> keycloakOptions
    )
    {
        _dbContextFactory = dbContextFactory;
        _client = keycloakOptions.Value.Audience;
        _realm = keycloakOptions.Value.Realm;
    }

    public async Task Consume(ConsumeContext<UserEvent> context)
    {
        var routingKey = context.RoutingKey();

        if (routingKey == $"KK.EVENT.ADMIN.{_realm}.SUCCESS.USER.CREATE")
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
        else if (routingKey == $"KK.EVENT.ADMIN.{_realm}.SUCCESS.USER.UPDATE")
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
        else if (routingKey == $"KK.EVENT.ADMIN.{_realm}.SUCCESS.USER.DELETE")
        {
            var typedEvent = context.GetMessage<UserDeletedEvent>();
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Users.Where(e => e.UserId == typedEvent.UserId).DeleteAsync();
            await dbContext.SaveChangesAsync();
        }
        else if (routingKey == $"KK.EVENT.CLIENT.{_realm}.SUCCESS.{_client}.REGISTER")
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
    }
}