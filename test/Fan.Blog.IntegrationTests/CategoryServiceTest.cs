using Fan.Blog.IntegrationTests.Base;
using Fan.Exceptions;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.IntegrationTests
{
    /// <summary>
    /// Category related business rules for blog service.
    /// </summary>
    public class CategoryServiceTest : BlogServiceIntegrationTestBase
    {
        /// <summary>
        /// Author can create a new category with just a title.
        /// </summary>
        [Fact]
        public async void Create_category_only_requires_title()
        {
            var cat = await _catSvc.CreateAsync("Technology");
            Assert.Equal(1, cat.Id);
            Assert.Equal("Technology", cat.Title);
            Assert.Equal("technology", cat.Slug);
            Assert.Null(cat.Description);
            Assert.Equal(0, cat.Count);
        }

        /// <summary>
        /// Delete the default category will throw exception.
        /// </summary>
        [Fact]
        public async void Default_category_cannot_be_deleted()
        {
            await Assert.ThrowsAsync<FanException>(() => _catSvc.DeleteAsync(1));
        }

        /// <summary>
        /// Cannot create a category with a title that exists already.
        /// </summary>
        [Fact]
        public async void Create_category_with_duplicate_title_throws_FanException()
        {
            // Given a category "Technology"
            SeedTestPost();

            // When create another category with the same title
            Task action() => _catSvc.CreateAsync(CAT_TITLE);

            // Then you got exception
            await Assert.ThrowsAsync<FanException>(action);

            // and you got msgs
            try
            {
                await action();
            }
            catch (FanException ex)
            {
                Assert.Equal($"'{CAT_TITLE}' already exists.", ex.Message);
            }
        }

        /// <summary>
        /// Cannot have a category with a title that exists already.
        /// </summary>
        [Fact]
        public async void Update_category_with_duplicate_title_throws_FanException()
        {
            // Given 2 categories
            await _catSvc.CreateAsync("Tech");
            await _catSvc.CreateAsync("Tech!!");

            // When update the title of one of them to be the same as other one
            var cat = await _catSvc.GetAsync(2);
            cat.Title = "Tech";
            Task action() => _catSvc.UpdateAsync(cat);

            // Then you got exception
            await Assert.ThrowsAsync<FanException>(action);

            // and error message
            try
            {
                await _catSvc.UpdateAsync(cat);
            }
            catch (FanException ex)
            {
                Assert.Equal("'Tech' already exists.", ex.Message);
            }
        }

        /// <summary>
        /// Author can update an existing category's title, description.
        /// </summary>
        [Fact]
        public async void Update_category_title_will_generate_new_slug()
        {
            // Given a category "Technology"
            SeedTestPost();
            var cat = await _catSvc.GetAsync(CAT_SLUG);
            Assert.Equal(1, cat.Id);
            Assert.Equal("Technology", cat.Title);

            // When author updates the category
            cat.Title = "Music";
            cat.Description = "A music category.";
            cat = await _catSvc.UpdateAsync(cat);

            // Then the category's slug will be updated too
            Assert.Equal(1, cat.Id);
            Assert.Equal("Music", cat.Title);
            Assert.Equal("music", cat.Slug);
            Assert.Equal("A music category.", cat.Description);
        }

        /// <summary>
        /// Test <see cref="BlogService.GetCategoryAsync(string)"/> either by id or by slug throws exception if not found.
        /// </summary>
        [Fact]
        public async void Get_category_throws_FanException_if_not_found()
        {
            await Assert.ThrowsAsync<FanException>(() => _catSvc.GetAsync(100));
            await Assert.ThrowsAsync<FanException>(() => _catSvc.GetAsync("slug-not-exist"));
        }

        /// <summary>
        /// All categories will have unique slug.
        /// </summary>
        /// <remarks>
        /// Create category with different title from an existing category but potentially 
        /// will result in the same slug, the algorithm will make it unique.
        /// </remarks>
        [Fact]
        public async void Category_slug_is_guaranteed_to_be_unique()
        {
            // Given an existing category "Technology"
            SeedTestPost();

            // When user creates a different category "Technology!!!"
            var cat = await _catSvc.CreateAsync("Technology!!!");

            // Then category will be created with an unique slug
            Assert.Equal("technology-2", cat.Slug);
        }

        /// <summary>
        /// Create category with a title in Chinese yields a 6-char random string.
        /// </summary>
        /// <remarks>
        /// User can name a category in any language, the algorithm for generating slug will give a 6-char slug.
        /// </remarks>
        [Fact]
        public async void Category_with_Chinese_title_results_random_6_char_slug()
        {
            // When creating a category with a chinese title
            var cat = await _catSvc.CreateAsync("你好");

            // Then you end up with a 6-char random string
            Assert.Equal(6, cat.Slug.Length);
        }

        /// <summary>
        /// To create a category user only needs to input a title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="expectSlug"></param>
        [Fact]
        public async void Category_title_with_pound_sign_will_turn_into_letter_s()
        {
            // When create a category named "C#"
            var category = await _catSvc.CreateAsync("C#");

            // Then you end up with "cs" as its slug
            Assert.Equal("cs", category.Slug);
        }

        /// <summary>
        /// When create a category with html in the title or description, they will be cleaned out.
        /// </summary>
        [Fact]
        public async void Category_title_and_description_will_be_cleaned_off_of_any_html_tags()
        {
            // When create a category with html in title or description
            var category = await _catSvc.CreateAsync("<h1>Test</h1>", "<p>This is a test category.</p>");

            // Then you end up with clean ones
            Assert.Equal("Test", category.Title);
            Assert.Equal("This is a test category.", category.Description);
        }
    }
}
