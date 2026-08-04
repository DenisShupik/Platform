using System.Text.Json;
using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Extensions;

namespace IntegrationTests.Tests;

public sealed class SearchCursorSerializationTests
{
    [Test]
    public async Task SearchCursor_CanBeSerializedInSearchResults()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web).ApplyCoreServiceOptions();
        var results = new SearchResultsDto
        {
            Items = [],
            NextCursor = SearchCursor.From("opaque-cursor")
        };

        var json = JsonSerializer.Serialize(results, options);

        await Assert.That(json).Contains("opaque-cursor");
    }
}
