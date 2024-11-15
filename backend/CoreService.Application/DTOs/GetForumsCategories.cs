using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetForumsCategoriesLatestByPostRequest
{
    [FromRoute] public LongIds ForumIds { get; set; }
    [FromQuery] public int? Count { get; set; }
}