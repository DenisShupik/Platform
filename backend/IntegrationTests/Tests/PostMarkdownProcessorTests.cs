using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Markdown;
using CoreService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Tests;

public sealed class PostMarkdownProcessorTests
{
    private readonly PostMarkdownProcessor _processor = new(Options.Create(new PostContentPolicyOptions()));

    [Test]
    public async Task Process_UsesVisibleMarkdownTextForSearchProjection()
    {
        var result = _processor.Process(
            PostContent.From("**Visible** [label](https://hidden.example/path) and `code`."));

        await Assert.That(result.ValueOrErrors(out var processedContent, out _)).IsTrue();
        await Assert.That(processedContent!.SearchText).IsEqualTo("Visible label and code.");
    }

    [Test]
    public async Task Process_RejectsRawHtml()
    {
        var result = _processor.Process(PostContent.From("<script>alert(1)</script>"));

        await Assert.That(result.ValueOrErrors(out _, out _)).IsFalse();
    }
}
