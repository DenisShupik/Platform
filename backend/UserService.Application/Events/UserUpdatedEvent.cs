namespace UserService.Application.Events;

public sealed class UserUpdatedEvent
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email{ get; set; }
}