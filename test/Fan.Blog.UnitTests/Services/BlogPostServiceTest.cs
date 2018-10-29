using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.UnitTests.Base;
using Fan.Blog.UnitTests.Helpers;
using Fan.Exceptions;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    public class BlogPostServiceTest : BlogServiceUnitTestBase
    {
        public BlogPostServiceTest()
        {
            _postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.BlogPost))
                .Returns(Task.FromResult(new Post() { Category = new Category { Title = "Test" } }));
        }

        /// <summary>
        /// When you publish a blog post the title cannot be empty.
        /// </summary>
        [Theory]
        [InlineData(null, EPostStatus.Published, "Blog post title cannot be empty.")]
        [InlineData("", EPostStatus.Published, "Blog post title cannot be empty.")]
        public async void Create_post_throws_FanException_if_post_status_is_published_and_title_is_empty(string title, EPostStatus status, string expectedMessages)
        {
            // Arrange: a blog post with an invalid title
            var blogPost = new BlogPost { Title = title, UserId = Actor.AUTHOR_ID, Status = status };

            // Act: validate
            await Assert.ThrowsAsync<FanException>(() => _blogPostSvc.CreateAsync(blogPost));

            try
            {
                await _blogPostSvc.CreateAsync(blogPost);
            }
            catch (FanException ex)
            {
                Assert.Equal(expectedMessages, ex.Message);
            }
        }

        /// <summary>
        /// If your post is a draft, then the title can be empty.
        /// </summary>
        [Fact]
        public async void Blog_post_title_can_be_empty_if_status_is_draft()
        {
            // Arrange a post with title null
            var blogPost = new BlogPost { Title = null, UserId = Actor.AUTHOR_ID, Status = EPostStatus.Draft };

            // Act
            await _blogPostSvc.CreateAsync(blogPost);
        }

        /// <summary>
        /// A blog post title cannot be over 256 characters limit.
        /// </summary>
        [Fact]
        public async void Post_Title_Cannot_Exceed_256_Chars_Length()
        {
            // Arrange: a blog post with a title of 257 chars
            var title = string.Join("", Enumerable.Repeat<char>('a', 257));
            var blogPost = new BlogPost { Title = title, UserId = Actor.AUTHOR_ID };

            // Act
            await Assert.ThrowsAsync<FanException>(() => _blogPostSvc.CreateAsync(blogPost));

            try
            {
                await _blogPostSvc.CreateAsync(blogPost);
            }
            catch (FanException ex)
            {
                Assert.Equal($"Blog post title cannot exceed {BlogPostService.TITLE_MAXLEN} chars.", ex.Message);
            }
        }
    }
}
