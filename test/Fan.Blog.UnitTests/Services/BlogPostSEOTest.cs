using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.UnitTests.Base;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    /// <summary>
    /// Unit tests post title and slug.
    /// </summary>
    public class BlogPostSEOTest : BlogServiceUnitTestBase
    {
        /// <summary>
        /// This test is to make sure the following, <see cref="https://github.com/FanrayMedia/Fanray/issues/88"/>
        /// 
        /// 1. User publishes a new post(at this point, a slug has been created, search engine could have scrawled it)
        /// 2. User goes back to change the post title
        /// 3. Publish the post again will not alter the slug, thus will not break SEO
        /// </summary>
        [Fact]
        public async void Update_post_title_will_not_alter_slug()
        {
            // 1. user writes a post with title
            var title = "A blog post title";
            var dt = DateTimeOffset.Now;
            var postId = 1;
            // Very important to setup return null for Post or it'll go into infinite loop
            _postRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult((Post)null));

            // 2. user publishes the post
            var slug = await _blogPostSvc.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // 3. user goes back to update post title
            // NOTE: at the point the existing slug is being passed in 
            // See BlogService.PrepPostAsync()
            var theSlug = await _blogPostSvc.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal(theSlug, slug);
        }

        /// <summary>
        /// This test is to make sure the following, <see cref="https://github.com/FanrayMedia/Fanray/issues/88"/>
        /// 
        /// 1. User publishes a new post(at this point, a slug has been created, search engine could have scrawled it)
        /// 2. User goes back and change the post slug
        /// 3. Publish the post again will be able to update slug
        /// </summary>
        [Fact]
        public async void Update_post_slug_will_alter_slug()
        {
            // 1. user writes a post with title
            var title = "A blog post title";
            var dt = DateTimeOffset.Now;
            var postId = 1;
            // Very important to setup return null for Post or it'll go into infinite loop
            _postRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult((Post)null));

            // 2. user publishes the post
            var slug = await _blogPostSvc.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // Now the user update the post slug
            slug = "i-want-a-different-slug-for-this-post";

            // 3. user goes back to update post title
            // NOTE: at the point the existing slug is being passed in 
            // See BlogService.PrepPostAsync()
            var theSlug = await _blogPostSvc.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal(theSlug, slug);
        }

        /// <summary>
        /// When create a post the slug is guaranteed to be unique.
        /// </summary>
        [Theory]
        [InlineData("A blog post title", "a-blog-post-title", "a-blog-post-title-2")]
        [InlineData("A blog post title 2", "a-blog-post-title-2", "a-blog-post-title-3")]
        public async void Create_post_will_always_produce_unique_slug(string title, string slug, string expected)
        {
            // Given an existing post with slug 
            var dt = DateTimeOffset.Now;
            _postRepoMock.Setup(r => r.GetAsync(slug, dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult(new Post { Id = 10000, Slug = slug }));

            // When user publishes the post that will conflict with existing slug
            var postId = 1;
            var slugUnique = await _blogPostSvc.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // Then a unique slug is produced
            Assert.Equal(expected, slugUnique);
        }

        /// <summary>
        /// If user manually updates an existing post's slug and it conflicts with an existing post,
        /// my blog will resolve the conflict by producing an unique slug automatically.
        /// </summary>
        [Fact]
        public async void Update_post_will_produce_unique_slug_if_user_updates_slug_to_run_into_conflict()
        {
            // Given an existing post
            var slug = "i-want-a-different-slug-for-this-post";
            var dt = DateTimeOffset.Now;
            _postRepoMock.Setup(r => r.GetAsync(slug, dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult(new Post { Id = 10000, Slug = slug }));

            // When user publishes the post and update the slug
            var postId = 1;
            var title = "A blog post title";
            var slugCreated = await _blogPostSvc.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);
            Assert.Equal("a-blog-post-title", slugCreated);

            // Now the user update the post slug
            slugCreated = "i-want-a-different-slug-for-this-post";

            // Then
            var slugUpdated = await _blogPostSvc.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal("i-want-a-different-slug-for-this-post-2", slugUpdated);
        }
    }
}
