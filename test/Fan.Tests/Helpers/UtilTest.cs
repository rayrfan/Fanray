using Fan.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
