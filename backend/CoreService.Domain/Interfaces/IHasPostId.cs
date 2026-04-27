using CoreService.Domain.ValueObjects;

namespace CoreService.Domain.Interfaces;

public interface IHasPostId
{
    PostId PostId { get; }
}