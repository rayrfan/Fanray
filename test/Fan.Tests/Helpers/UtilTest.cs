using Fan.Helpers;
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
    }
}
