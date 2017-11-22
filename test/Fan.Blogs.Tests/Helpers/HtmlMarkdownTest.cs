using Xunit;

namespace Fan.Blogs.Tests.Helpers
{
    /// <summary>
    /// Testing out html to markdown libraries: ReverseMarkdown and Html2Markdown.
    /// Currently I found both have issues with nested list.
    /// Html2Markdown https://github.com/baynezy/Html2Markdown/issues/73
    /// ReverseMarkdown https://github.com/mysticmind/reversemarkdown-net/issues/14
    /// </summary>
    public class HtmlMarkdownTest
    {
        string html = @"<ul><li>Item1<ul><li>Item2</li></ul></li></ul>";
        [Fact]
        public void NestedList_test_with_ReverseMarkdown()
        {
            var converter = new ReverseMarkdown.Converter();
            string result = converter.Convert(html);
        }

        [Fact]
        public void NestedList_test_with_Html2Markdown()
        {
            var converter = new Html2Markdown.Converter();
            string result = converter.Convert(html);
        }
    }
}
