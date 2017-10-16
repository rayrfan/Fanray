using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Fan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fan.Blogs.Tests.Services.IntegrationTests
{
    /// <summary>
    /// Integration tests for <see cref="Fan.Blogs.Services.BlogService"/> the different scenarios an author posts.
    /// </summary>
    public class BlogPostIntegrationTest : BlogIntegrationTestBase
    {
        /// <summary>
        /// When an author publishes a blog post from OLW.
        /// </summary>
        [Fact]
        public async void Author_Can_Publish_BlogPost_From_OLW()
        {
            // Arrange
            SeedTestPost();
            var blogPost = new BlogPost // A user posts this from OLW
            {
                UserId = Actor.AUTHOR_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryTitle = null,               // user didn't input
                TagTitles = null,                   // user didn't input
                CreatedOn = new DateTimeOffset(),   // user didn't input, it's MinValue
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            };

            // Act
            var result = await _blogSvc.CreatePostAsync(blogPost);

            // Assert
            Assert.Equal(2, result.Id);
            Assert.Equal("hello-world", result.Slug);
            Assert.NotEqual(DateTimeOffset.MinValue, result.CreatedOn);
            Assert.Equal(1, result.Category.Id);
            Assert.Equal(0, result.Tags.Count);
        }

        /// <summary>
        /// When an author publishes a blog post from browser.
        /// </summary>
        [Fact]
        public async void Author_Can_Publish_BlogPost_From_Browser()
        {
            // Arrange
            SeedTestPost();
            var createdOn = DateTimeOffset.Now; // user local time
            var blogPost = new BlogPost // A user posts this from browser
            {
                UserId = Actor.AUTHOR_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,
                TagTitles = null,                   // user didn't input
                CreatedOn = createdOn,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            };

            // Act
            var result = await _blogSvc.CreatePostAsync(blogPost);

            // Assert
            Assert.Equal(2, result.Id);
            Assert.Equal("hello-world", result.Slug);
            Assert.Equal(createdOn.ToUniversalTime(), result.CreatedOn);
            Assert.Equal(1, result.Category.Id);
            Assert.Equal(0, result.Tags.Count);
        }

        /// <summary>
        /// When an author publishes a blog post with a new category and tags from OLW.
        /// </summary>
        [Fact]
        public async void Author_Publish_BlogPost_With_New_Category_And_Tags_From_OLW()
        {
            // Arrange
            SeedTestPost();
            var blogPost = new BlogPost // A user posts this from OLW
            {
                UserId = Actor.AUTHOR_ID,
                Title = "Hello World!",
                Slug = null,
                Body = "This is my first post",
                Excerpt = null,
                CategoryTitle = "Travel",
                TagTitles = new List<string> { "Windows 10", TAG2_TITLE },
                CreatedOn = new DateTimeOffset(),
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            };

            // Act
            var result = await _blogSvc.CreatePostAsync(blogPost);

            // Assert
            var cats = await _blogSvc.GetCategoriesAsync();
            var tags = await _blogSvc.GetTagsAsync();

            // BlogPost
            Assert.Equal(2, result.Id);
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.Equal("cs", result.Tags[1].Slug);

            // Category
            Assert.Equal(2, cats.Count); // there are now 2 cats
            Assert.Equal(1, cats[1].Count);

            // Tags
            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(2, tags.Find(t => t.Title == TAG2_TITLE).Count); // C# has 2 posts
        }

        /// <summary>
        /// When an author updates a blog post from OLW.
        /// </summary>
        [Fact]
        public async void Author_Can_Update_BlogPost_From_OLW()
        {
            // Arrange
            SeedTestPost();
            var blogPost = await _blogSvc.GetPostAsync(1);
            var wasCreatedOn = blogPost.CreatedOn;

            // Act
            blogPost.CategoryTitle = "Travel"; // new cat
            blogPost.TagTitles = new List<string> { "Windows 10", TAG2_TITLE }; // 1 new tag, 1 existing
            blogPost.CreatedOn = DateTimeOffset.Now; // update the post time to now, user local time

            var result = await _blogSvc.UpdatePostAsync(blogPost);

            // Assert
            var cats = await _blogSvc.GetCategoriesAsync();
            var tags = await _blogSvc.GetTagsAsync();

            // BlogPost
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Title == TAG2_TITLE));
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Slug == "windows-10"));

            // Category
            Assert.Equal(2, cats.Count); // there are now 2 cats
            Assert.Equal(0, cats[0].Count);
            Assert.Equal(1, cats[1].Count);

            // Tags
            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(1, tags.Find(t => t.Title == TAG2_TITLE).Count); // C# has 1 post

            // CreatedOn & UpdatedOn
            Assert.True(result.CreatedOn > wasCreatedOn);
            Assert.Null(result.UpdatedOn);
        }

        /// <summary>
        /// When an author updates a blog post to a draft from browser.
        /// </summary>
        [Fact]
        public async void Author_Can_Update_BlogPost_To_Draft_From_Browser()
        {
            // Arrange
            SeedTestPost();
            var blogPost = await _blogSvc.GetPostAsync(1);
            var wasCreatedOn = blogPost.CreatedOn;

            // Act
            blogPost.CategoryTitle = "Travel"; // new cat
            blogPost.TagTitles = new List<string> { "Windows 10", TAG2_TITLE }; // 1 new tag, 1 existing
            blogPost.CreatedOn = DateTimeOffset.Now; // update the post time to now, user local time
            blogPost.Status = EPostStatus.Draft;

            var result = await _blogSvc.UpdatePostAsync(blogPost);

            // Assert
            var cats = await _blogSvc.GetCategoriesAsync();
            var tags = await _blogSvc.GetTagsAsync();

            // BlogPost
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Title == TAG2_TITLE));
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Slug == "windows-10"));

            // Category
            Assert.Equal(2, cats.Count); // there are now 2 cats
            Assert.Equal(0, cats[0].Count);
            Assert.Equal(0, cats[1].Count); // a draft is not counted

            // Tags
            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(0, tags.Find(t => t.Title == TAG2_TITLE).Count); // draft is not counted

            // CreatedOn & UpdatedOn
            Assert.True(result.CreatedOn > wasCreatedOn);
            Assert.True(result.UpdatedOn == result.CreatedOn);
        }

        /// <summary>
        /// A blog post datetime is in humanized string, such as "now", "an hour ago".
        /// </summary>
        [Fact]
        public async void Visitor_Views_BlogPost_Date_In_Humanized_String()
        {
            // Arrange
            SeedTestPost();
            var blogPost = new BlogPost // A user posts this from browser
            {
                UserId = Actor.AUTHOR_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,
                TagTitles = null,                   // user didn't input
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now      // user local time
            };

            // Act
            var postNow = await _blogSvc.CreatePostAsync(blogPost);

            // Assert
            Assert.Equal("now", postNow.CreatedOnFriendly);
        }
    }
}
