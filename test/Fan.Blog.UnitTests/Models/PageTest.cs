using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.UnitTests.Base;
using Fan.Exceptions;
using Markdig;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Models
{
    /// <summary>
    /// Unit tests for key <see cref="Page"/> properties such as title and slug.
    /// </summary>
    public class PageTest : BlogUnitTestBase
    {
        [Fact]
        public async void Page_draft_can_have_empty_title()
        {
            // When you have a draft with empty title
            var page = new Page { Title = "", Status = EPostStatus.Draft };

            // Then its validation will not fail
            await page.ValidateTitleAsync();
        }

        /// <summary>
        /// Slugs may get url encoded which may exceed max length, if that happens the slug is trimmed.
        /// </summary>
        [Fact]
        public void Page_with_Chinese_title_produces_UrlEncoded_slug_which_may_get_trimmed()
        {
            // Given a page title of 30 Chinese chars, which will translate into a slug over 250 
            // chars due to url encoding
            var pageTitle = string.Join("", Enumerable.Repeat<char>('验', 30));
            var page = new Page {
                Title = pageTitle,
            };

            // When the title is processed, I expect the slug to be 250 char as follows
            // '验' -> "%E9%AA%8C" 9 chars, 27 * 9 + 7 = 250
            var expectedSlug = WebUtility.UrlEncode(string.Join("", Enumerable.Repeat<char>('验', 27))) + "%E9%AA%";
            Assert.Equal(250, expectedSlug.Length);

            // Then the slug comes out to be 250 long
            Assert.Equal(expectedSlug, PageService.SlugifyPageTitle(page.Title));
        }

        /// <summary>
        /// Page slug is calculated based on its title, on rare occassions a title could result in
        /// a conflict with an existing slug.
        /// </summary>
        [Fact]
        public async void Page_title_resulting_duplicate_slug_throws_FanException()
        {
            // Given a post slug with max length of 250 chars
            var slug = WebUtility.UrlEncode(string.Join("", Enumerable.Repeat<char>('验', 27))) + "%E9%AA%";
            IList<Post> list = new List<Post> {
                new Post { Slug = slug, Type = EPostType.Page, Id = 1 },
            };
            _postRepoMock.Setup(repo => repo.GetListAsync(It.IsAny<PostListQuery>())).Returns(Task.FromResult((list, 1)));

            // When create/update a page title that conflits the existing slug
            // Then you get FanException
            var givenTitle = string.Join("", Enumerable.Repeat<char>('验', 30));
            var page = new Page { Title = givenTitle };
            var slug2 = PageService.SlugifyPageTitle(page.Title);
            await Assert.ThrowsAsync<FanException>(() => _pageSvc.EnsurePageSlugAsync(slug2, page));
        }

        /// <summary>
        /// Unlike a blog post, user cannot specify a page slug because a parent page's navigation depends on 
        /// its children's titles to calc their slugs.
        /// </summary>
        [Fact]
        public void Page_navigation_is_tranformed_to_HTML_based_on_child_page_titles()
        {
            var parentSlug = "docs";
            var navMd = "- [[Getting Started]] \n- [[Deploy to Azure]]";

            var actual = PageService.NavMdToHtml(navMd, parentSlug).Replace("\n", "");
            var expected = @"<ul><li><a href=""/docs/getting-started"" title=""Getting Started"">Getting Started</a></li><li><a href=""/docs/deploy-to-azure"" title=""Deploy to Azure"">Deploy to Azure</a></li></ul>";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Test convert source code md to html by Markdig library.
        /// </summary>
        /// <remarks>
        /// Currently this is not used as html is gotten directly from the editor preview.
        /// TODO try https://github.com/pauldotknopf/Pek.Markdig.HighlightJs
        /// </remarks>
        [Fact]
        public void SourceCode_markdown_is_tranformed_to_pre_code_html_by_Markdig()
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
