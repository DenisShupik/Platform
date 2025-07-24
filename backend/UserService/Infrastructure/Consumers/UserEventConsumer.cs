using System.Text.Json;
using System.Text.Json.Nodes;
using LinqToDB;
using UserService.Domain.Entities;
using UserService.Infrastructure.Events;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Consumers;

public sealed class UserEventConsumer(IServiceProvider serviceProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async ValueTask ConsumeAsync(JsonNode @event, CancellationToken cancellationToken)
    {
        var jsonObject = @event.AsObject();
        var type = jsonObject["type"]?.GetValue<string>();

        switch (type)
        {
            case "REGISTER":
            {
                var typedEvent = jsonObject.Deserialize<UserRegisteredEvent>(JsonOptions);
                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var user = new User
                {
                    UserId = typedEvent.UserId,
                    Username = typedEvent.Details.Username,
                    Email = typedEvent.Details.Email,
                    Enabled = true,
                    CreatedAt = typedEvent.RegisteredAt
                };
                await dbContext.Users.AddAsync(user, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                return;
            }
        }

        var resourceType = jsonObject["resourceType"]?.GetValue<string>();

        switch (resourceType)
        {
            case "USER":
            {
                var operationType = jsonObject["operationType"]?.GetValue<string>();
                switch (operationType)
                {
                    case "CREATE":
                    {
                        var typedEvent = jsonObject.Deserialize<UserCreatedEvent>(JsonOptions);
                        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        var user = new User
                        {
                            UserId = typedEvent.UserId,
                            Username = typedEvent.Representation.Username,
                            Email = typedEvent.Representation.Email,
                            Enabled = typedEvent.Representation.Enabled,
                            CreatedAt = typedEvent.CreatedAt,
                        };
                        await dbContext.Users.AddAsync(user, cancellationToken);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        return;
                    }
                    case "UPDATE":
                    {
                        var typedEvent = jsonObject.Deserialize<UserUpdatedEvent>(JsonOptions);
                        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        await dbContext.Users
                            .Where(e => e.UserId == typedEvent.Representation.UserId)
                            .Set(e => e.Username, typedEvent.Representation.Username)
                            .Set(e => e.Email, typedEvent.Representation.Email)
                            .Set(e => e.Enabled, typedEvent.Representation.Enabled)
                            .UpdateAsync(token: cancellationToken);
                        return;
                    }
                    case "DELETE":
                    {
                        var typedEvent = jsonObject.Deserialize<UserDeletedEvent>(JsonOptions);
                        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        await dbContext.Users.Where(e => e.UserId == typedEvent.UserId).DeleteAsync(cancellationToken);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        return;
                    }
                }

                break;
            }
        }
    }
}