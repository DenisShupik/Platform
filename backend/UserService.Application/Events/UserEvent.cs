namespace UserService.Application.Events;

public sealed class UserEvent
{
    public enum ResourceTypes
    {
        User,
    }

    public enum OperationTypes
    {
        Create,
        Update,
        Delete
    }

    public ResourceTypes ResourceType { get; set; }
    public OperationTypes OperationType { get; set; }
    public string ResourcePath { get; set; }
    public string Representation { get; set; }
}