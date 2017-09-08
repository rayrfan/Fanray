using Fan.Helpers;
using System.Collections.Generic;
using Xunit;

namespace Fan.Tests.Helpers
{
    public class UtilTest
    {
        /// <summary>
        /// Test for <see cref="Util.FormatSlug(string)"/>.
        /// </summary>
        [Theory]
        [InlineData("c#", "c")]
        [InlineData(" It'll rain , tomorrow-", "itll-rain-tomorrow")]
        [InlineData("my 1.0 release", "my-1-0-release")]
        [InlineData("1+1=2", "11-2")]
        [InlineData("1.1-1_1_.-", "1-1-1-1")]
        [InlineData("你好。", "")]
        [InlineData("<script>A post title</script>", "scripta-post-title-script")]
        public void FormatSlug_Test(string slug, string expected)
        {
            Assert.Equal(expected, Util.FormatSlug(slug));
        }

        /// <summary>
        /// Test for <see cref="Util.GetExcerpt(string, int)"/> method.
        /// TODO should have a performance test on GetExcerpt.
        /// </summary>
        [Theory]
        [InlineData("<p>A body less than 55 words</p>", "A body less than 55 words", 55)]
        [InlineData("<p>A body more than 5 words.</p>", "A body more than 5 words.", 6)]
        [InlineData("<p>A body more than 5 words.</p>", "A body more than 5…", 5)]
        [InlineData("<p></p>", "", 55)]
        public void GetExcerpt_Test(string body, string expected, int wordLimit)
        {
            Assert.Equal(expected, Util.GetExcerpt(body, wordLimit));
        }

        /// <summary>
        /// Test <see cref="Util.FormatTaxonomySlug(string, IEnumerable{string})"/> for
        /// long, duplicate user inputs.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        /// <param name="existingSlugs"></param>
        [Theory]
        [InlineData("c#", "cs")]
        [InlineData("this is a really long category title", "this-is-a-really-long-ca")]
        [InlineData("cat1", "cat1-2", new string[] { "cat1" })]
        public void FormatTaxonomySlug_Test(string input, string expected, IEnumerable<string> existingSlugs = null)
        {
            Assert.Equal(expected, Util.FormatTaxonomySlug(input, existingSlugs));
        }
    }
}
