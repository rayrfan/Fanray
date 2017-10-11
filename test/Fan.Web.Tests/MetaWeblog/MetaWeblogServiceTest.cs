using Fan.Data;
using Fan.Services;
using Fan.Tests;
using Fan.Tests.Data;
using Fan.Web.MetaWeblog;
using Fan.Web.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fan.Web.Tests.MetaWeblog
{
    /// <summary>
    /// Integration tests for <see cref="MetaWeblogService"/> class.
    /// </summary>
    public class MetaWeblogServiceTest : DataTestBase
    {
        MetaWeblogService _svc;
        public MetaWeblogServiceTest()
        {
            // repos
            var catRepo = new SqlCategoryRepository(_db);
            var tagRepo = new SqlTagRepository(_db);
            var metaRepo = new SqlMetaRepository(_db);
            var postRepo = new SqlPostRepository(_db);

            // loggers
            var loggerBlogSvc = _loggerFactory.CreateLogger<BlogService>();
            var loggerSettingSvc = _loggerFactory.CreateLogger<SettingService>();
            var loggerMetaSvc = _loggerFactory.CreateLogger<MetaWeblogService>();

            // blog svc
            var settingSvc = new SettingService(metaRepo, _cache, loggerSettingSvc);
            var blogSvc = new BlogService(settingSvc, catRepo, metaRepo, postRepo, tagRepo, _cache, loggerBlogSvc, _mapper);

            // metaweblog svc
            var envMock = new Mock<IHostingEnvironment>();
            var context = new Mock<HttpContext>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
            _svc = new MetaWeblogService(new FakeUserManager(), new FakeSignInManager(contextAccessor.Object), blogSvc, settingSvc, loggerMetaSvc, envMock.Object);
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
                PostDate = new DateTime(), // ??
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
                PostDate = new DateTime(), // ??
                PostId = null, // ??
                Publish = true,
                Slug = null,
                Tags = new List<string> { DataTestBase.TAG1_TITLE },
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
                PostDate = new DateTime(), // ??
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
            var result = await _svc.GetCategoriesAsync(blogId, userName, password, rootUrl);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal($"{rootUrl}/category/{DataTestBase.CAT_SLUG}", result[0].HtmlUrl);
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
            Assert.True(result.Contains(DataTestBase.TAG1_TITLE));
            Assert.True(result.Contains(DataTestBase.TAG2_TITLE));
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
