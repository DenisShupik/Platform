using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.Dtos;

public enum SearchResultType : byte
{
    Forum = 0,
    Category = 1,
    Thread = 2,
    Post = 3
}

public sealed class SearchResultsDto
{
    public required IReadOnlyList<SearchResultDto> Items { get; init; }
    public required SearchCursor? NextCursor { get; init; }
}

[Include(
    typeof(Forum),
    PropertyGenerationMode.AsRequired,
    nameof(Forum.ForumId),
    nameof(Forum.CreatedBy),
    nameof(Forum.CreatedAt))]
public sealed partial class SearchResultDto
{
    public required SearchResultType Type { get; init; }
    public required ForumTitle ForumTitle { get; init; }
    public required CategoryId? CategoryId { get; init; }
    public required CategoryTitle? CategoryTitle { get; init; }
    public required ThreadId? ThreadId { get; init; }
    public required ThreadTitle? ThreadTitle { get; init; }
    public required PostId? PostId { get; init; }
    public required string? Snippet { get; init; }
    public required float Rank { get; init; }
}
