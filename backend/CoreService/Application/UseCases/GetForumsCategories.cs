using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesLatestByPostRequest
{
    [FromRoute] public GuidIdList<ForumId> ForumIds { get; set; }
    [FromQuery] public int? Count { get; set; }
}