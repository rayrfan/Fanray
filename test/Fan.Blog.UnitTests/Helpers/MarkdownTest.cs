using Fan.Blog.Helpers;
using Markdig;
using Xunit;

namespace Fan.Blog.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="BlogUtil.TocToHtml(string)"/>.
    /// </summary>
    public partial class BlogUtilTest
    {
        /// <summary>
        /// Test convert TOC md to html.
        /// </summary>
        [Fact]
        public void Toc_Test()
        {
            var md = "- [[Test Page 1]] \n- [[Test Page 2]]";

            var actual = BlogUtil.TocToHtml(md).Replace("\n", "");
            var expected = @"<ul><li><a href=""/test-page-1"" title=""Test Page 1"">Test Page 1</a></li><li><a href=""/test-page-2"" title=""Test Page 2"">Test Page 2</a></li></ul>";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Test convert source code md to html.
        /// </summary>
        /// <remarks>
        /// TODO try https://github.com/pauldotknopf/Pek.Markdig.HighlightJs
        /// </remarks>
        [Fact]
        public void SourceCode_md()
        {
            var md = @"```cs
var i = 5;
```";
            var actual = Markdown.ToHtml(md).Replace("\n", "");
            var expected = @"<pre><code class=""language-cs"">var i = 5;</code></pre>";
            Assert.Equal(expected, actual);
        }
    }
}
