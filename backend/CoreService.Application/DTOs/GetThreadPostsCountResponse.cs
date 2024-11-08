namespace CoreService.Application.DTOs;

public sealed class GetThreadPostsCountResponse
{
    public long ThreadId { get; set; }
    public long Count { get; set; }
}