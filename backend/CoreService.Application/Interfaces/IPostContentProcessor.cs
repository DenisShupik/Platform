using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public readonly record struct ProcessedPostContent(
    PostContent Content,
    string SearchText);

public interface IPostContentProcessor
{
    Result<ProcessedPostContent, InvalidPostContentError> Process(PostContent content);
}
