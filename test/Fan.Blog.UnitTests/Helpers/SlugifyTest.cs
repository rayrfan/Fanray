using Fan.Blog.Helpers;
using Fan.Blog.Services;
using System.Collections.Generic;
using Xunit;

namespace Fan.Blog.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="BlogUtil.SlugifyTaxonomy(string, int, IEnumerable{string})"/>.
    /// </summary>
    public partial class BlogUtilTest
    {
        /// <summary>
        /// When user inputs a long title, it will result a long slug, <see cref="BlogUtil.SlugifyTaxonomy(string, int, IEnumerable{string})"/>
        /// will truncate the slug and keep <see cref="CategoryService.SLUG_MAXLEN"/> or <see cref="TagService.SLUG_MAXLEN"/>
        /// number of characters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        /// <param name="existingSlugs"></param>
        [Fact]
        public void SlugifyTaxonomy_trucates_long_slug_and_keeps_only_24_chars()
        {
            var expected = "this-is-a-really-long-ca";
            var actual = BlogUtil.SlugifyTaxonomy("this is a really long category title", CategoryService.SLUG_MAXLEN);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// When inputted title has "#" it will be replaced by letter "s".
        /// </summary>
        [Fact]
        public void SlugifyTaxonomy_replaces_hashtag_with_letter_s()
        {
            var expected = "cs";
            var actual = BlogUtil.SlugifyTaxonomy("c#", CategoryService.SLUG_MAXLEN);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Not everything could be slugified, for example given a title in Chinese,
        /// this method will return a 6-char randon string.
        /// </summary>
        [Fact]
        public void SlugifyTaxonomy_produces_8chars_string_when_input_is_non_english()
        {
            var actual = BlogUtil.SlugifyTaxonomy("你好", CategoryService.SLUG_MAXLEN);
            Assert.True(actual.Length == 6);
        }

        /// <summary>
        /// When user give a title to category or tag, their according service will generate a slug
        /// based on the title. So the title inputted may not exist but the logic to generate the
        /// slug could still output a slug that exists, for example "c#" and "cs" are two titles that
        /// will generate "cs" slug.  Therefore this util method make the slug unique by appending a
        /// counter to it, the counter starts with 2.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
        /// <param name="existing"></param>
        [Theory]
        [InlineData("c#", "cs-2", new string[] { "cs" })]
        [InlineData("cs", "cs-2", new string[] { "cs" })]
        [InlineData("cat", "cat-2", new string[] { "cat" })]
        [InlineData("cat-2", "cat-3", new string[] { "cat-2" })]
        public void SlugifyTaxonomy_returns_unique_slug_by_append_counter_to_duplicate(string input, string expected, IEnumerable<string> existing = null)
        {
            Assert.Equal(expected, BlogUtil.SlugifyTaxonomy(input, CategoryService.SLUG_MAXLEN, existing));
        }
    }
}
