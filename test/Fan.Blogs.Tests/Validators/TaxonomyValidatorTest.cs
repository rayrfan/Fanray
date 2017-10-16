using Xunit;
using System.Collections.Generic;
using System.Linq;
using Fan.Enums;
using Fan.Blogs.Validators;
using Fan.Blogs.Enums;
using Fan.Blogs.Models;

namespace Fan.Blogs.Tests.Validators
{
    /// <summary>
    /// Unit tests for <see cref="TaxonomyValidator"/>.
    /// </summary>
    public class TaxonomyValidatorTest
    {
        private TaxonomyValidator _validator;
        public TaxonomyValidatorTest()
        {
            // prep a validator with an existing title
            _validator = new TaxonomyValidator(new List<string> { "Technology" }, ETaxonomyType.Category);
        }

        /// <summary>
        /// Test TaxonomyValidator for when a category has a null or empty title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="numberOfErrors"></param>
        /// <param name="expectedMessages"></param>
        [Theory]
        [InlineData(null, 1, new string[] { "'Title' should not be empty." })]
        [InlineData("", 2, new string[] { "'Title' should not be empty.", "'Title' must be between 1 and 24 characters. You entered 0 characters." })]
        public async void Taxonomy_Title_Cannot_Be_Null_Or_Empty(string title, int numberOfErrors, string[] expectedMessages)
        {
            // Arrange: a cat with an invalid title
            var cat = new Category { Title = title };

            // Act: validate
            var result = await _validator.ValidateAsync(cat);

            // Assert: number of errors and messages
            Assert.Equal(numberOfErrors, result.Errors.Count);
            Assert.Equal(expectedMessages[0], result.Errors[0].ErrorMessage);
            if (numberOfErrors > 1)
                Assert.Equal(expectedMessages[1], result.Errors[1].ErrorMessage);
        }

        /// <summary>
        /// Test TaxonomyValidator for when a category title that is over 24 characters limit.
        /// </summary>
        [Fact]
        public async void Taxonomy_Title_Cannot_Exceed_24_Chars_Length()
        {
            // Arrange: a cat with a title of 25 chars
            var title = string.Join("", Enumerable.Repeat<char>('a', 25));
            var cat = new Category { Title = title };

            // Act: validate
            var result = await _validator.ValidateAsync(cat);

            // Assert: 1 error
            Assert.Equal(1, result.Errors.Count);
            Assert.Equal("'Title' must be between 1 and 24 characters. You entered 25 characters.", result.Errors[0].ErrorMessage);
        }

        /// <summary>
        /// Test TaxonomyValidator for a title that already exists.
        /// </summary>
        [Fact]
        public async void Taxonomy_Title_Must_Be_Unique_Case_Insensitive()
        {
            // Arrange: a cat with title that exists already, notice: title is case insensitive
            var cat = new Category { Title = "technology" };
            
            // Act: validate
            var result = await _validator.ValidateAsync(cat);

            // Assert: 1 error
            Assert.Equal(1, result.Errors.Count);
            Assert.Equal("Category 'technology' is not available, please choose a different one.", result.Errors[0].ErrorMessage);
        }
    }
}