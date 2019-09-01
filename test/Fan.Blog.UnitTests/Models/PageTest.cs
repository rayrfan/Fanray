using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.UnitTests.Base;
using Fan.Exceptions;
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
        /// Unlike a blog post, user cannot specify a page slug because a parent page's TOC depends on 
        /// its children's titles to calc their slugs.
        /// </summary>
        [Fact]
        public async void Toc_is_calucated_based_on_child_page_titles()
        {
            // Given a post with TOC
            var toc = "- [[Test Page 1]] \n- [[Test Page 2]]";
            var post = new Post { Toc = toc, Type = EPostType.Page, Id = 1 };
            _postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.Page)).Returns(Task.FromResult(post));

            // When the page is retrieved
            var page = await _pageSvc.GetAsync(1);

            // Then the toc is transformed into html through the TocHtml prop
            var expected = @"<ul>
<li><a href=""/test-page-1"" title=""Test Page 1"">Test Page 1</a></li>
<li><a href=""/test-page-2"" title=""Test Page 2"">Test Page 2</a></li>
</ul>
";
            Assert.Equal(expected.Replace("\r", ""), page.TocHtml);
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
            Assert.Equal(expectedSlug, BlogUtil.SlugifyPageTitle(page.Title));
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
            var slug2 = BlogUtil.SlugifyPageTitle(page.Title);
            await Assert.ThrowsAsync<FanException>(() => _pageSvc.EnsurePageSlugAsync(slug2, page));
        }
    }
}
