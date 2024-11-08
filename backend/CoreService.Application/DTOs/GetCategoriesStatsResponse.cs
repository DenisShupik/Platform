namespace CoreService.Application.DTOs;

public sealed class CategoryStats
{
    public long CategoryId { get; set; }
    public long ThreadCount { get; set; }
    public long PostCount { get; set; }
}