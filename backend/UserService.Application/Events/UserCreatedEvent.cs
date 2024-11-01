namespace UserService.Application.Events;

public sealed class UserCreatedEvent
{
    public string Username { get; set; }
}