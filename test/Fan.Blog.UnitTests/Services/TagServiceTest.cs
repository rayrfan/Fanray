using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.UnitTests.Base;
using Fan.Data;
using Fan.Exceptions;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="TagService"/>.
    /// </summary>
    public class TagServiceTest : BlogServiceUnitTestBase
    {
        /// <summary>
        /// Setting up existing tags.
        /// </summary>
        public TagServiceTest()
        {
            // blog settings
            _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings", EMetaType.Setting))
                .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));

            // mocks
            var tag = new Tag { Id = 1, Title = "technology", Slug = "technology" };
            _tagRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(new List<Tag> { tag }));
            _tagRepoMock.Setup(r => r.GetAsync(1)).Returns(Task.FromResult(tag));
        }

        /// <summary>
        /// User can only input title and description no slug when create a tag, <see cref="ITagService.CreateAsync(Tag)"/>.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="expectSlug"></param>
        [Theory]
        [InlineData("Web Development!", "web-development")]
        [InlineData("C#", "cs")]
        public async void User_can_only_input_tag_Title_not_Slug(string title, string expectSlug)
        {
            // Arrange: a tag with a title no slug, and a repo to return it
            var tag = new Tag { Title = title };
            _tagRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Tag>())).Returns(Task.FromResult(tag));

            // Act: when we create it
            tag = await _tagSvc.CreateAsync(tag);

            // Assert: then it has the expected slug
            Assert.Equal(expectSlug, tag.Slug);
        }

        /// <summary>
        /// Test <see cref="ITagService.CreateAsync(Tag)"/> throws excpetion if title exists already.
        /// </summary>
        [Fact]
        public async void CreateTag_Throws_FanException_If_Title_Already_Exist()
        {
            // Arrange: a category with a title that exists
            var tag = new Tag { Title = "Technology" };

            // Act and Assert: when we create it, we get exception
            await Assert.ThrowsAsync<FanException>(() => _tagSvc.CreateAsync(tag));

            // Act and Assert: error message
            try
            {
                await _tagSvc.CreateAsync(tag);
            }
            catch (FanException ex)
            {
                Assert.Equal("'Technology' already exists.", ex.Message);
            }
        }

        /// <summary>
        /// Test <see cref="ITagService.CreateAsync(Tag)"/> a title in Chinese with no manual slug yields a 6-char random string.
        /// </summary>
        /// <remarks>
        /// User can name a category in any language, the algorithm for generating slug will give a 6-char slug.
        /// </remarks>
        [Fact]
        public async void CreateTag_Chinese_Title_Without_Slug_Results_6_Char_Random_Slug()
        {
            // Arrange: a category with Chinese title no slug
            var tag = new Tag { Title = "你好" };
            _tagRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Tag>())).Returns(Task.FromResult(tag));

            // Act: when we create it
            tag = await _tagSvc.CreateAsync(tag);

            // Assert: then it has the expected slug length
            Assert.Equal(6, tag.Slug.Length);
        }

        /// <summary>
        /// Test <see cref="ITagService.CreateAsync(Tag)"/> would call TagRepository's CreateAsync and invalidates cache for all tags.
        /// </summary>
        [Fact]
        public async void CreateTag_Calls_TagRepository_CreateAsync_And_Invalidates_Cache_For_AllTags()
        {
            // Arrange 
            var tag = new Tag { Title = "Tag1" };

            // Act
            await _tagSvc.CreateAsync(tag);

            // Assert
            _tagRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Tag>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_TAGS));
        }

        /// <summary>
        /// Test <see cref="ITagService.DeleteAsync(int)"/> calls TagRepository Delete method.
        /// </summary>
        [Fact]
        public async void DeleteTag_Calls_TagRepository_DeleteAsync_And_Invalidates_Cache_For_AllTags()
        {
            // Act
            await _tagSvc.DeleteAsync(1);

            // Assert
            _tagRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_TAGS));
        }

        /// <summary>
        /// Test <see cref="ITagService.GetBySlugAsync(string)"/> either by id or by slug throws exception if not found.
        /// </summary>
        [Fact]
        public async void GetTagBySlug_Throws_FanException_If_Not_Found()
        {
            await Assert.ThrowsAsync<FanException>(() => _tagSvc.GetAsync(100));
            await Assert.ThrowsAsync<FanException>(() => _tagSvc.GetBySlugAsync("slug-not-exist"));
        }

        /// <summary>
        /// Test updating an existing tag with new title, <see cref="ITagService.UpdateAsync(Tag)"/>.
        /// </summary>
        [Fact]
        public async void Update_an_existing_tag_with_new_title()
        {
            // Arrange: get the mocked tag
            var techTag = await _tagSvc.GetAsync(1);

            // Act: update
            techTag.Title = "Tech";
            techTag = await _tagSvc.UpdateAsync(techTag);

            // Assert
            Assert.Equal("Tech", techTag.Title);
            Assert.Equal("tech", techTag.Slug);
        }

        /// <summary>
        /// <see cref="ITagService.UpdateAsync(Tag)"/> treats tag title insensitively.
        /// </summary>
        [Fact]
        public async void Update_tag_with_title_changed_only_in_casing_is_OK()
        {
            // Arrange: get the mocked tag
            var tag = await _tagSvc.GetAsync(1);
            Assert.Equal("technology", tag.Title); // notice title is lowercase

            // Act: here I change technology to Technology
            tag.Title = "Technology"; 

            // Update the same tag is ok
            var tagAgain = await _tagSvc.UpdateAsync(tag);
            Assert.Equal(1, tagAgain.Id);
            Assert.Equal("Technology", tagAgain.Title);
        }

        /// <summary>
        /// Test <see cref="ITagService.UpdateAsync(Tag)"/> would call TagRepository's UpdateAsync and invalidates cache for all tags.
        /// </summary>
        [Fact]
        public async void UpdateTag_Calls_TagRepository_And_Invalidates_Cache_For_AllTags()
        {
            // Arrange 
            var tag = await _tagSvc.GetAsync(1);

            // Act
            await _tagSvc.UpdateAsync(tag);

            // Assert
            _tagRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogCache.KEY_ALL_TAGS));
        }
    }
}
