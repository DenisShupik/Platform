using System.Text.Json;
using System.Text.Json.Nodes;
using LinqToDB;
using UserService.Domain.DomainEvents;
using UserService.Domain.Entities;
using UserService.Infrastructure.Events;
using UserService.Infrastructure.Persistence;
using Wolverine;

namespace UserService.Infrastructure.Consumers;

public sealed class UserEventConsumer(IServiceProvider serviceProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async ValueTask ConsumeAsync(JsonNode @event, CancellationToken cancellationToken)
    {
        if (@event.GetValueKind() != JsonValueKind.Object) return;

        var jsonObject = @event.AsObject();
        var type = jsonObject["type"]?.GetValue<string>();

        switch (type)
        {
            case "REGISTER":
            {
                var typedEvent = jsonObject.Deserialize<UserRegisteredEvent>(JsonOptions);
                if (typedEvent == null) return;
                var dbContext = serviceProvider.GetRequiredService<WriteApplicationDbContext>();
                var user = new User(
                    typedEvent.UserId,
                    typedEvent.Details.Username,
                    typedEvent.Details.Email,
                    true,
                    typedEvent.RegisteredAt
                );
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
                        if (typedEvent == null) return;
                        var dbContext = serviceProvider.GetRequiredService<WriteApplicationDbContext>();
                        var user = new User(
                            typedEvent.UserId,
                            typedEvent.Representation.Username,
                            typedEvent.Representation.Email,
                            typedEvent.Representation.Enabled,
                            typedEvent.CreatedAt
                        );
                        await dbContext.Users.AddAsync(user, cancellationToken);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        return;
                    }
                    case "UPDATE":
                    {
                        var typedEvent = jsonObject.Deserialize<UserUpdatedEvent>(JsonOptions);
                        if (typedEvent == null) return;
                        var dbContext = serviceProvider.GetRequiredService<WriteApplicationDbContext>();
                        var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
                        await dbContext.Users
                            .Where(e => e.UserId == typedEvent.Representation.UserId)
                            .Set(e => e.Username, typedEvent.Representation.Username)
                            .Set(e => e.Email, typedEvent.Representation.Email)
                            .Set(e => e.Enabled, typedEvent.Representation.Enabled)
                            .UpdateAsync(cancellationToken);
                        await messageBus.PublishAsync(new UserUpdatedDomainEvent
                            { UserId = typedEvent.Representation.UserId });
                        return;
                    }
                    case "DELETE":
                    {
                        var typedEvent = jsonObject.Deserialize<UserDeletedEvent>(JsonOptions);
                        if (typedEvent == null) return;
                        var dbContext = serviceProvider.GetRequiredService<WriteApplicationDbContext>();
                        await dbContext.Users
                            .Where(e => e.UserId == typedEvent.UserId)
                            .DeleteAsync(cancellationToken);
                        return;
                    }
                }

                break;
            }
        }
    }
}