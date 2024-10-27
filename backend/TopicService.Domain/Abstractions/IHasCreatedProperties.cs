namespace TopicService.Domain.Abstractions;

public interface IHasCreatedProperties
{
    DateTime Created { get; set; }
    Guid CreatedBy { get; set; }
}