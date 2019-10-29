using Fan.Blog.Enums;
using Fan.Blog.IntegrationTests.Base;
using Fan.Blog.IntegrationTests.Helpers;
using Fan.Blog.Models;
using Fan.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fan.Blog.IntegrationTests.Services
{
    /// <summary>
    /// Integration tests for <see cref="Blog.Services.BlogPostService"/> testing the different 
    /// scenarios a user authors blog posts.
    /// </summary>
    public class BlogPostServiceTest : BlogServiceIntegrationTestBase
    {
        /// <summary>
        /// When an author publishes a blog post from OLW.
        /// </summary>
        [Fact]
        public async void Admin_publishes_BlogPost_from_OLW()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();

            // When user creates a new blog post from OLW
            var result = await _blogPostSvc.CreateAsync(new BlogPost // from OLW
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryTitle = null,               // user didn't input
                TagTitles = null,                   // user didn't input
                CreatedOn = new DateTimeOffset(),   // user didn't input, it's MinValue
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Then
            Assert.Equal(2, result.Id);
            Assert.Equal("hello-world", result.Slug);
            Assert.NotEqual(DateTimeOffset.MinValue, result.CreatedOn);
            Assert.Equal(1, result.Category.Id);
            Assert.Empty(result.Tags);
        }

        /// <summary>
        /// When an author publishes a blog post from browser.
        /// </summary>
        [Fact]
        public async void Admin_publishes_BlogPost_from_browser()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();

            // When user creates a new blog post from browser with new a new tag "test"
            var createdOn = DateTimeOffset.Now; // user local time
            var blogPost = await _blogPostSvc.CreateAsync(new BlogPost // from browser
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,
                TagTitles = new List<string> { "test", TAG2_TITLE },
                //TagTitles = null,                 // user didn't input
                CreatedOn = createdOn,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });
            var tags = await _tagSvc.GetAllAsync();

            // Then
            Assert.Equal(2, blogPost.Id);
            Assert.Equal("hello-world", blogPost.Slug);
            Assert.Equal(createdOn.ToUniversalTime(), blogPost.CreatedOn);
            Assert.Equal(1, blogPost.Category.Id);
            Assert.Equal(2, blogPost.Tags.Count);
            Assert.Contains(blogPost.Tags, t => t.Title.Equals("test"));
            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(2, tags.Find(t => t.Title == TAG2_TITLE).Count); // C# has 2 posts
            Assert.Equal(1, tags.Find(t => t.Title == "test").Count); // test has 1 post
        }

        /// <summary>
        /// When an author publishes a blog post with a new category and tags from OLW.
        /// </summary>
        [Fact]
        public async void Admin_publishes_BlogPost_with_new_Category_and_Tag_from_OLW()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();

            // When user creates a new blog post from OLW 
            // with a new cat "Travel" and new tag "Windows 10"
            var result = await _blogPostSvc.CreateAsync(new BlogPost // A user posts this from OLW
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = null,
                Body = "This is my first post",
                Excerpt = null,
                CategoryTitle = "Travel",
                TagTitles = new List<string> { "Windows 10", TAG2_TITLE },
                Tags = await _tagSvc.GetAllAsync(),
                CreatedOn = new DateTimeOffset(),
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Then
          
            // BlogPost
            Assert.Equal(2, result.Id);
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.Equal("cs", result.Tags[1].Slug);

            // Cats and Tags
            var cats = await _catSvc.GetAllAsync();
            var tags = await _tagSvc.GetAllAsync();

            Assert.Equal(2, cats.Count); // there are now 2 cats
            Assert.Equal(1, cats[1].Count);
            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(2, tags.Find(t => t.Title == TAG2_TITLE).Count); // C# has 2 posts
        }

        /// <summary>
        /// When an author updates a blog post from OLW.
        /// </summary>
        [Fact]
        public async void Admin_updates_BlogPost_from_OLW()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();
            var blogPost = await _blogPostSvc.GetAsync(1);
            var wasCreatedOn = blogPost.CreatedOn;

            // When user updates the post from OLW
            blogPost.CategoryTitle = "Travel"; // new cat
            blogPost.TagTitles = new List<string> { "Windows 10", TAG2_TITLE }; // 1 new tag, 1 existing
            blogPost.Tags = await _tagSvc.GetAllAsync();
            blogPost.CreatedOn = DateTimeOffset.Now; // update the post time to now, user local time

            var result = await _blogPostSvc.UpdateAsync(blogPost);

            // Then

            // BlogPost
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Title == TAG2_TITLE));
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Slug == "windows-10"));

            var cats = await _catSvc.GetAllAsync();
            var tags = await _tagSvc.GetAllAsync();

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
        public async void Admin_updates_BlogPost_to_draft_from_browser()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();
            var blogPost = await _blogPostSvc.GetAsync(1);
            var wasCreatedOn = blogPost.CreatedOn;

            // When user updates blog post to draft
            blogPost.CategoryTitle = "Travel"; // new cat
            blogPost.TagTitles = new List<string> { "Windows 10", TAG2_TITLE }; // 1 new tag, 1 existing
            blogPost.Tags = await _tagSvc.GetAllAsync();
            blogPost.CreatedOn = DateTimeOffset.Now; // update the post time to now, user local time
            blogPost.Status = EPostStatus.Draft;

            var result = await _blogPostSvc.UpdateAsync(blogPost);

            // Then

            // BlogPost
            Assert.Equal(2, result.Category.Id);
            Assert.Equal("travel", result.Category.Slug);
            Assert.Equal(2, result.Tags.Count);
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Title == TAG2_TITLE));
            Assert.NotNull(result.Tags.SingleOrDefault(t => t.Slug == "windows-10"));

            // Category and Tags
            var cats = await _catSvc.GetAllAsync();
            var tags = await _tagSvc.GetAllAsync();

            Assert.Equal(2, cats.Count); // there are now 2 cats
            Assert.Equal(0, cats[0].Count);
            Assert.Equal(0, cats[1].Count); // a draft is not counted

            Assert.Equal(3, tags.Count); // there are now 3 tags
            Assert.Equal(0, tags.Find(t => t.Title == TAG2_TITLE).Count); // draft is not counted

            // CreatedOn & UpdatedOn
            Assert.True(result.CreatedOn > wasCreatedOn);
            Assert.True(result.UpdatedOn.HasValue);
        }

        /// <summary>
        /// A blog post datetime is in humanized string, such as "now", "an hour ago".
        /// </summary>
        [Fact]
        public async void Visitor_sees_BlogPost_date_in_humanized_string()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();

            // When user creates a new blog post
            var postNow = await _blogPostSvc.CreateAsync(new BlogPost // A user posts this from browser
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,
                TagTitles = null,                   // user didn't input
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
                CreatedOn = DateTimeOffset.Now      // user local time
            });

            var coreSettings = new CoreSettings();
            // Then the date displays human friend string
            Assert.Equal("now", postNow.CreatedOn.ToDisplayString(coreSettings.TimeZoneId));
        }

        /// <summary>
        /// When author saves a draft with empty title, it's OK.
        /// </summary>
        [Fact]
        public async void Admin_Can_Save_Draft_With_Empty_Title_And_Slug()
        {
            // Given 1 blog post with 1 cat and 2 tags exist in db
            Seed_1BlogPost_with_1Category_2Tags();

            // When user creates a new blog post
            var createdOn = DateTimeOffset.Now; // user local time
            var result = await _blogPostSvc.CreateAsync(new BlogPost // A user posts this from browser
            {
                UserId = Actor.ADMIN_ID,
                Title = null,
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,
                TagTitles = null,                   // user didn't input
                CreatedOn = createdOn,
                Status = EPostStatus.Draft,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Then
            Assert.Equal(2, result.Id);
            Assert.Null(result.Slug);
            Assert.Null(result.Title);
            Assert.Equal(createdOn.ToUniversalTime(), result.CreatedOn);
            Assert.Equal(1, result.Category.Id);
            Assert.Empty(result.Tags);
        }
    }
}
