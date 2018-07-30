using Fan.Blog.UnitTests.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Validators;
using System.Linq;
using Xunit;

namespace Fan.Blogs.UnitTests.Validators
{
    public class PostValidatorTest
    {
        PostValidator _validator;
        public PostValidatorTest()
        {
            _validator = new PostValidator();
        }

        /// <summary>
        /// Test PostValidator for when a blog post has a null or empty title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="numberOfErrors"></param>
        /// <param name="expectedMessages"></param>
        [Theory]
        [InlineData(null, 1, new string[] { "'Title' should not be empty." })]
        [InlineData("", 2, new string[] { "'Title' should not be empty.", "'Title' must be between 1 and 256 characters. You entered 0 characters." })]
        public async void Post_Title_Cannot_Be_Null_Or_Empty(string title, int numberOfErrors, string[] expectedMessages)
        {
            // Arrange: a blog post with an invalid title
            var blogPost = new BlogPost { Title = title, UserId = Actor.AUTHOR_ID };

            // Act: validate
            var result = await _validator.ValidateAsync(blogPost);

            // Assert: number of errors and messages
            Assert.Equal(numberOfErrors, result.Errors.Count);
            Assert.Equal(expectedMessages[0], result.Errors[0].ErrorMessage);
            if (numberOfErrors > 1)
                Assert.Equal(expectedMessages[1], result.Errors[1].ErrorMessage);
        }

        /// <summary>
        /// Test PostValidator for when a post title that is over 256 characters limit.
        /// </summary>
        [Fact]
        public async void Post_Title_Cannot_Exceed_256_Chars_Length()
        {
            // Arrange: a blog post with a title of 257 chars
            var title = string.Join("", Enumerable.Repeat<char>('a', 257));
            var blogPost = new BlogPost { Title = title, UserId = Actor.AUTHOR_ID };

            // Act: validate
            var result = await _validator.ValidateAsync(blogPost);

            // Assert: 1 error
            Assert.Equal(1, result.Errors.Count);
            Assert.Equal("'Title' must be between 1 and 256 characters. You entered 257 characters.", result.Errors[0].ErrorMessage);
        }
    }
}
