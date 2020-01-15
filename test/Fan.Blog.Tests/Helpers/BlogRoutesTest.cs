using Fan.Blog.Helpers;
using System;
using Xunit;

namespace Fan.Blog.Tests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="BlogRoutes"/> class.
    /// </summary>
    public class BlogRoutesTest
    {
        /// <summary>
        /// Test <see cref="BlogRoutes.GetPostRelativeLink(DateTimeOffset, string)"/> method.
        /// </summary>
        [Fact]
        public void GetPostRelativeLink_returns_link_starts_with_slash_and_2digit_month_day()
        {
            var createdOn = new DateTimeOffset(2018, 9, 9, 0, 0, 0, TimeSpan.Zero);
            var slug = "my-post";
            var relativeLink = BlogRoutes.GetPostRelativeLink(createdOn, slug);

            Assert.StartsWith("/", relativeLink);
            Assert.Equal("/post/2018/09/09/my-post", relativeLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetPostPreviewRelativeLink(DateTimeOffset, string)"/> method.
        /// </summary>
        [Fact]
        public void GetPostPreviewRelativeLink_returns_relative_link_starts_with_slash()
        {
            var createdOn = new DateTimeOffset(2018, 9, 9, 0, 0, 0, TimeSpan.Zero);
            var slug = "my-post";
            var relativeLink = BlogRoutes.GetPostPreviewRelativeLink(createdOn, slug);

            Assert.StartsWith("/", relativeLink);
            Assert.Equal("/preview/post/2018/09/09/my-post", relativeLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetPostPermalink(int)"/> method.
        /// </summary>
        [Fact]
        public void GetPostPermalink_returns_relative_url_starts_with_slash()
        {
            var postId = 1;
            var permalink = BlogRoutes.GetPostPermalink(postId);

            Assert.Equal("/blog/post/1", permalink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetPostEditLink(int)"/> method.
        /// </summary>
        [Fact]
        public void GetPostEditLink_returns_relative_url_starts_with_slash()
        {
            var postId = 1;
            var editLink = BlogRoutes.GetPostEditLink(postId);

            Assert.Equal("/admin/compose/post/1", editLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetCategoryRelativeLink(string)"/> method.
        /// </summary>
        [Fact]
        public void GetCategoryRelativeLink_returns_url_starts_with_slash()
        {
            var slug = "technology";
            var relativeLink = BlogRoutes.GetCategoryRelativeLink(slug);

            Assert.Equal("/blog/technology", relativeLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetCategoryRssRelativeLink(string)"/> method.
        /// </summary>
        [Fact]
        public void GetCategoryRssRelativeLink_returns_url_starts_with_slash()
        {
            var slug = "technology";
            var rssLink = BlogRoutes.GetCategoryRssRelativeLink(slug);

            Assert.Equal("/blog/technology/feed", rssLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetTagRelativeLink(string)"/> method.
        /// </summary>
        [Fact]
        public void GetTagRelativeLink_returns_url_starts_with_slash()
        {
            var slug = "asp-net-core";
            var relativeLink = BlogRoutes.GetTagRelativeLink(slug);

            Assert.Equal("/posts/tagged/asp-net-core", relativeLink);
        }

        /// <summary>
        /// Test <see cref="BlogRoutes.GetArchiveRelativeLink(int, int)"/> method.
        /// </summary>
        [Fact]
        public void GetArchiveRelativeLink_returns_link_starts_with_slash_and_2digit_month()
        {
            var year = 2018;
            var month = 9;
            var relativeLink = BlogRoutes.GetArchiveRelativeLink(year, month);

            Assert.StartsWith("/", relativeLink);
            Assert.Equal("/posts/2018/09", relativeLink);
        }
    }
}
