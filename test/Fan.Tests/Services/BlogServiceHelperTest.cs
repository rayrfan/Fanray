using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Fan.Services;

namespace Fan.Tests.Services
{
    public class BlogServiceHelperTest
    {
        /// <summary>
        /// Test <see cref="BlogServiceHelper.FormatTaxonomySlug(string, IEnumerable{string})"/> for
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
            Assert.Equal(expected, BlogServiceHelper.FormatTaxonomySlug(input, existingSlugs));
        }
    }
}
