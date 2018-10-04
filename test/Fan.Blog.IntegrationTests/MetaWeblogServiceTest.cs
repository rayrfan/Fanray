using Fan.Blog.Helpers;
using Fan.Blog.IntegrationTests.Base;
using Fan.Blog.IntegrationTests.Helpers;
using Fan.Blog.MetaWeblog;
using Fan.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Fan.Blog.IntegrationTests
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

            // UserService
            var loggerUserSvc = _loggerFactory.CreateLogger<UserService>();
            var userSvc = new UserService(new FakeUserManager(), loggerUserSvc);

            // metaweblog svc
            var context = new Mock<HttpContext>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
            _svc = new MetaWeblogService(
                userSvc,
                new FakeSignInManager(contextAccessor.Object),
                _blogSvc,
                _settingSvcMock.Object,
                loggerMetaSvc);
        }

        const string APP_KEY = "appKey";
        const string USERNAME = Actor.AUTHOR_USERNAME;
        const string PASSWORD = "password";
        const string ROOT_URL = "http://localhost";
        const string BLOG_ID = "1";

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
            var result = await _svc.NewPostAsync(BLOG_ID, USERNAME, PASSWORD, metaPost, publish: true);

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
            var result = await _svc.NewPostAsync(BLOG_ID, USERNAME, PASSWORD, metaPost, publish: true);

            // Assert
            Assert.Equal("2", result); // new blog post with id 2

            var tags = await _svc.GetKeywordsAsync(BLOG_ID, USERNAME, PASSWORD);
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
            var result = await _svc.EditPostAsync("1", USERNAME, PASSWORD, metaPost, publish: true);

            // Assert
            var metaPostAgain = await _svc.GetPostAsync("1", USERNAME, PASSWORD, ROOT_URL);
            Assert.Equal("Windows 10", metaPostAgain.Categories[0]);
        }

        [Fact]
        public async void DeletePostAsync_Test()
        {
            // Arrange
            SeedTestPost();

            // Act
            await _svc.DeletePostAsync(APP_KEY, "1", USERNAME, PASSWORD);

            // Assert
            await Assert.ThrowsAsync<MetaWeblogException>(() => _svc.GetPostAsync("1", USERNAME, PASSWORD, ROOT_URL));
        }

        [Fact]
        public async void GetRecentPostsAsync_Test()
        {
            // Arrange
            SeedTestPosts(11);

            // Act
            var result = await _svc.GetRecentPostsAsync(BLOG_ID, USERNAME, PASSWORD, int.MaxValue, ROOT_URL);

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
            var metaCatList = await _svc.GetCategoriesAsync(BLOG_ID, USERNAME, PASSWORD, ROOT_URL);
            var catUrl = BlogRoutes.GetCategoryRelativeLink(CAT_SLUG);

            // Assert
            Assert.Single(metaCatList);
            Assert.Equal($"{ROOT_URL}{catUrl}", metaCatList[0].HtmlUrl);
        }

        [Fact]
        public async void GetKeywordsAsync_Test()
        {
            // Arrange
            SeedTestPosts(11);

            // Act
            var result = await _svc.GetKeywordsAsync(BLOG_ID, USERNAME, PASSWORD);

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
            var result = await _svc.GetUsersBlogsAsync(APP_KEY, USERNAME, PASSWORD, ROOT_URL);

            // Assert
            Assert.Equal("Fanray", result[0].BlogName);
        }

        /// <summary>
        /// Author can upload image from OLW.
        /// </summary>
        [Fact]
        public async void Author_can_upload_images_from_OLW()
        {
            // Given an existing image
            string base64 = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAYAAACM/rhtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAN8SURBVFhHzZjNS1RRFMDvuTOOlQsNIsiyEGoVrgoCRWQQMtRwCoKgNhNEf0DRpkWLolUFbmZRayNaaJRCLiIsjILaOEQLJbMS0T7IZgydmXdv59x737w31qjvQ3q/4cw959773jvvnPs1A7NnmyeA8RZGSBRQmoZswq6rYltM9DYNTI9oS3N9rL2HMxgG1Re/1L1dD5Co60aNskmhL91PSpHlpEtsVEIfW7dtd10Vey2wm9O34lkuvWzbdbokYPZM8wQWLVjzlHH+UNV6BIT1uPHexxljKq6NJ/fFSqXjxvQIpDCAnehTtuygYOxW08CHS6bHf+XGWPtNxvhFxijFUcUMz+g6SOMQC66HYhTRnnHXRI8c5Ft0U2yIbooxfGoMRjbF6B355nsdLAzu6JCC7zbmmkBcfEmkvj035rrodRBwHZRZ3ymWjF+GGBsgYaasJlKA9w0A9+nAKaYbuF/Qtu06R/fxFLyQrgowSZwrcWP/jVv8AmplcdvY86fp6gF9gAgQQedK1O4kmvbuqSa12xfOma4egKARdAFMwOG3xaqSZCXTc+OY9+dc8jTIWEeNjGV0lQ9wxsg3h2oq5Kr/TQC4zIAUHVimfWd4ZXDnCE60btLxbDmHxTTpDtBfe3L+gTF8E2iro+GhhgiwXSittqDjrRJkIzUFJZCDFP5/SZj4vt+qFL/ESI6qBoPFYbQuNf/amL6B86+Gk2CJ+nhcTmaO9L0z9etCDuLrddMb4op1u/bEV9yawqHrxdxB/EV3gAm+yLkl+jESQ0UL0qbdE6EsU6soiVJaSDZkgeg3Y9BvpnExDXvQGei25Jx2MMBDzM/X0LFvqx30+xD6p2ATI4hHwAARxO0No2fhy5FsShydFPsg0bD1VGIb1NeQbKm/YqpDx6QYN1OlbBxIzizzrvklJd1TK6Y6dPRxC6ci5TuK6OMWfvnO9Saj/cIwRimC7mBFMnAULHtZ0A5GNMXOOohEcZKQc5FMsQ0FDS6MP5rAxaZFCjmO9hPV4pE4yPuZ1r4pYyqOPvu03+L8tDE9gXP2GB5D2lDNlh3UTdWh9VL9sU3HF7skUBXS6r3blqr4l79z7HMPBz781yaobqTVCt2msi7Ll3/lYoVcnhVySyhUusWpW6EyTzaKKk2ffJ79mJypw5s1uGVp6n1dMbfIivlV4q5bp72w+D32B8Tg4wsSF0ucAAAAAElFTkSuQmCC";
            string contentType = "image/png";
            string filename = "fanray logo.png";
            string filenameSlugged = "fanray-logo.png";
            SeedImages(filenameSlugged);

            // When user uploads an image with the same name
            var mediaInfo = await _svc.NewMediaObjectAsync(BLOG_ID, USERNAME, PASSWORD,
                new MetaMediaObject
                {
                    Name = filename,
                    Type = contentType,
                    Bits = Convert.FromBase64String(base64),
                },
                null);

            // image url
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");
            var expectedUrl = $"{STORAGE_ENDPOINT}/media/blog/{year}/{month}/fanray-logo-1.png";
            Assert.Equal(expectedUrl, mediaInfo.Url);

            // and storage provider is called only once since it's a tiny image
            _storageProviderMock.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<char>()),
                Times.Exactly(1));
        }
    }
}