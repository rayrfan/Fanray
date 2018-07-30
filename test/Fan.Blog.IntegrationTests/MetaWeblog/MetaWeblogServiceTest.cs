using Fan.Blogs.Helpers;
using Fan.Blogs.MetaWeblog;
using Fan.Blog.IntegrationTests.Fakes;
using Fan.Blog.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using Fan.Blog.IntegrationTests.Base;

namespace Fan.Blog.IntegrationTests.MetaWeblog
{
    /// <summary>
    /// Integration tests for <see cref="MetaWeblogService"/> class.
    /// </summary>
    public class MetaWeblogServiceTest : BlogServiceIntegrationTestBase
    {
        MetaWeblogService _svc;
        public MetaWeblogServiceTest()
        {
            // loggers
            var loggerMetaSvc = _loggerFactory.CreateLogger<MetaWeblogService>();

            // metaweblog svc
            var context = new Mock<HttpContext>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
            _svc = new MetaWeblogService(new FakeUserManager(), new FakeSignInManager(contextAccessor.Object), 
                _blogSvc, _settingSvcMock.Object, _mediaSvcMock.Object, loggerMetaSvc);
        }

        string appKey = "appKey";
        string blogId = "blogId";
        string userName = Actor.AUTHOR_USERNAME;
        string password = "password";
        string rootUrl = "http://localhost";

        // -------------------------------------------------------------------- Posts

        [Fact]
        public async void NewPostAsync_Test()
        {
            // Arrange
            SeedTestPost();
            var metaPost = new MetaPost
            {
                AuthorId = Actor.AUTHOR_ID.ToString(),
                Categories = null,
                CommentPolicy = null, // ??
                Description = "<p>This is a post from OLW</p>",
                Excerpt = null,
                Link = null, // ??
                PostDate = new DateTimeOffset(), // ??
                PostId = null, // ??
                Publish = true,
                Slug = null,
                Tags = null,
                Title = "A post from OLW",
            };

            // Act
            var result = await _svc.NewPostAsync("1", userName, password, metaPost, publish: true);

            // Assert
            Assert.Equal("2", result); // new blog post with id 2
        }

        [Fact]
        public async void Author_Creates_NewPost_With_Existing_Tags()
        {
            // Arrange
            SeedTestPost();
            var metaPost = new MetaPost
            {
                AuthorId = Actor.AUTHOR_ID.ToString(),
                Categories = new List<string>(), // try empty
                CommentPolicy = null, // ??
                Description = "<p>This is a post from OLW</p>",
                Excerpt = null,
                Link = null, // ??
                PostDate = new DateTimeOffset(), // ??
                PostId = null, // ??
                Publish = true,
                Slug = null,
                Tags = new List<string> { TAG1_TITLE },
                Title = "A post from OLW",
            };

            // Act
            var result = await _svc.NewPostAsync("1", userName, password, metaPost, publish: true);

            // Assert
            Assert.Equal("2", result); // new blog post with id 2

            var tags = await _svc.GetKeywordsAsync("1", userName, password);
            Assert.Equal(2, tags.Count);
        }

        [Fact]
        public async void EditPostAsync_Test()
        {
            // Arrange
            SeedTestPost();
            var metaPost = new MetaPost
            {
                AuthorId = Actor.AUTHOR_ID.ToString(),
                Categories = new List<string> { "Windows 10" },
                CommentPolicy = null, // ??
                Description = "<p>This is a post from OLW</p>",
                Excerpt = null,
                Link = null, // ??
                PostDate = new DateTimeOffset(), // ??
                PostId = null, // ??
                Publish = true,
                Slug = null,
                Tags = null,
                Title = "A post from OLW",
            };

            // Act
            var result = await _svc.EditPostAsync("1", userName, password, metaPost, publish: true);

            // Assert
            var metaPostAgain = await _svc.GetPostAsync("1", userName, password, rootUrl);
            Assert.Equal("Windows 10", metaPostAgain.Categories[0]);
        }

        [Fact]
        public async void DeletePostAsync_Test()
        {
            // Arrange
            SeedTestPost();

            // Act
            await _svc.DeletePostAsync(appKey, "1", userName, password);

            // Assert
            await Assert.ThrowsAsync<MetaWeblogException>(() => _svc.GetPostAsync("1", userName, password, rootUrl));
        }

        [Fact]
        public async void GetRecentPostsAsync_Test()
        {
            // Arrange
            SeedTestPosts(11);

            // Act
            var result = await _svc.GetRecentPostsAsync(blogId, userName, password, int.MaxValue, rootUrl);

            // Assert
            Assert.Equal(11, result.Count);
        }

        // -------------------------------------------------------------------- Categories / Tags

        [Fact]
        public async void GetCategoriesAsync_Test()
        {
            // Arrange
            SeedTestPosts(11);

            // Act
            var metaCatList = await _svc.GetCategoriesAsync(blogId, userName, password, rootUrl);
            var catUrl = string.Format(BlogRoutes.CATEGORY_URL_TEMPLATE, CAT_SLUG);

            // Assert
            Assert.Single(metaCatList);
            Assert.Equal($"{rootUrl}/{catUrl}", metaCatList[0].HtmlUrl);
        }

        [Fact]
        public async void GetKeywordsAsync_Test()
        {
            // Arrange
            SeedTestPosts(11);

            // Act
            var result = await _svc.GetKeywordsAsync(blogId, userName, password);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(TAG1_TITLE, result);
            Assert.Contains(TAG2_TITLE, result);
        }

        // -------------------------------------------------------------------- Other

        [Fact]
        public async void GetUsersBlogsAsync_Test()
        {
            // Arrange
            SeedTestPost();

            // Act
            var result = await _svc.GetUsersBlogsAsync(appKey, userName, password, rootUrl);

            // Assert
            Assert.Equal("Fanray", result[0].BlogName);
        }
    }
}
