using System.Text.Json;
using LinqToDB;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Events;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;
using UserService.Presentation.Extensions;

namespace UserService.Presentation.Consumers;

public sealed class UserConsumer : IConsumer<UserEvent>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public UserConsumer(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task Consume(ConsumeContext<UserEvent> context)
    {
        var message = context.Message;
        if (message.ResourceType == UserEvent.ResourceTypes.User)
        {
            switch (message.OperationType)
            {
                case UserEvent.OperationTypes.Create:
                {
                    var userId = message.GetGuidFromResourcePath() ?? throw new InvalidOperationException();
                    var typedEvent =
                        JsonSerializer.Deserialize<UserCreatedEvent>(message.Representation, _jsonOptions) ??
                        throw new InvalidOperationException();
                    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                    var user = new User
                    {
                        UserId = userId,
                        Username = typedEvent.Username,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await dbContext.Users.AddAsync(user);
                    await dbContext.SaveChangesAsync();
                }
                    break;
                case UserEvent.OperationTypes.Update:
                {
                    var typedEvent =
                        JsonSerializer.Deserialize<UserUpdatedEvent>(message.Representation, _jsonOptions) ??
                        throw new InvalidOperationException();
                    await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                    await dbContext.Users
                        .Where(e => e.UserId == typedEvent.Id)
                        .Set(e => e.Username, typedEvent.Username)
                        .Set(e => e.Email, typedEvent.Email)
                        .UpdateAsync();
                }
                    break;
                case UserEvent.OperationTypes.Delete:
                default:
                    break;
            }
        }
    }
}