using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.UnitTests.Base;
using Fan.Exceptions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Models
{
    /// <summary>
    /// Unit tests for key <see cref="BlogPost"/> properties such as title, slug.
    /// </summary>
    public class BlogPostTest : BlogUnitTestBase
    {
        // -------------------------------------------------------------------- Title / Slug

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

        // -------------------------------------------------------------------- Post Validation

        /// <summary>
        /// If your post is a draft, then the title can be empty.
        /// </summary>
        [Fact]
        public async void BlogPost_draft_can_have_empty_title()
        {
            // When you have a draft with empty title
            var blogPost = new BlogPost { Title = "", Status = EPostStatus.Draft };

            // Then its validation will not fail
            await blogPost.ValidateTitleAsync();
        }

        /// <summary>
        /// When you publish a blog post the title cannot be empty.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="numberOfErrors"></param>
        /// <param name="expectedMessages"></param>
        [Theory]
        [InlineData(null, EPostStatus.Published, 1, new string[] { "'Title' must not be empty." })]
        [InlineData("", EPostStatus.Published, 1, new string[] { "'Title' must not be empty." })]
        public async void Publish_BlogPost_does_not_allow_empty_title(string title, EPostStatus status, int numberOfErrors, string[] expectedMessages)
        {
            // Given a blog post to publish
            var blogPost = new BlogPost { Title = title, Status = status };

            // When validate it throws FanException
            await Assert.ThrowsAsync<FanException>(() => blogPost.ValidateTitleAsync());

            // And with these number of errors and messages
            try
            {
                await blogPost.ValidateTitleAsync();
            }
            catch (FanException ex)
            {
                Assert.Equal(numberOfErrors, ex.ValidationErrors.Count);
                Assert.Equal(expectedMessages[0], ex.ValidationErrors[0].ErrorMessage);
            }
        }

        /// <summary>
        /// A blog post title cannot exceed 250 characters regardless whether it's published or draft.
        /// </summary>
        [Theory]
        [InlineData(EPostStatus.Draft, new string[] { "The length of 'Title' must be 250 characters or fewer. You entered 251 characters." })]
        [InlineData(EPostStatus.Published, new string[] { "The length of 'Title' must be 250 characters or fewer. You entered 251 characters." })]
        public async void BlogPost_title_cannot_exceed_250_chars_regardless_status(EPostStatus status, string[] expectedMessages)
        {
            // Arrange: a blog post with a title of 251 chars
            var title = string.Join("", Enumerable.Repeat<char>('a', 251));
            var blogPost = new BlogPost { Title = title, Status = status };

            // Act: validate
            await Assert.ThrowsAsync<FanException>(() => blogPost.ValidateTitleAsync());
            try
            {
                await blogPost.ValidateTitleAsync();
            }
            catch (FanException ex)
            {
                // Assert: 1 error
                Assert.Equal(1, ex.ValidationErrors.Count);
                Assert.Equal(expectedMessages[0], ex.ValidationErrors[0].ErrorMessage);
            }
        }

        /// <summary>
        /// When you pass <see cref="BlogPostService.CreateAsync(BlogPost)"/> a null param
        /// you get <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public async void CreateAsync_throws_ArgumentNullException_if_param_passed_in_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _blogPostSvc.CreateAsync(null));
        }

        /// <summary>
        /// When you pass <see cref="BlogPostService.UpdateAsync(BlogPost)"/> a null param
        /// you get <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public async void UpdateAsync_throws_ArgumentNullException_if_param_passed_in_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _blogPostSvc.UpdateAsync(null));
        }
    }
}
