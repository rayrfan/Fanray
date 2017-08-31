using Fan.Exceptions;
using Fan.Models;
using Fan.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Tests.Services
{
    public class TagTest : BlogServiceTest
    {
        /// <summary>
        /// Setting up existing tags.
        /// </summary>
        public TagTest()
        {
            // blog settings
            _metaRepoMock.Setup(repo => repo.GetAsync("BlogSettings"))
                .Returns(Task.FromResult(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) }));

            // 
            _tagRepoMock.Setup(r => r.GetListAsync()).Returns(Task.FromResult(
                new List<Tag>
                {
                    new Tag { Title = "Technology", Slug = "tech" }
                }    
            ));
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateTagAsync(Tag)"/> with user input only title but no slug.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="expectSlug"></param>
        [Theory]
        [InlineData("Web Development!", "web-development")]
        [InlineData("C#", "cs")]
        [InlineData("Tech", "tech-2")]
        public async void CreateTag_User_Input_Only_Title_No_Slug(string title, string expectSlug)
        {
            // Arrange: a tag with a title no slug, and a repo to return it
            var tag = new Tag { Title = title };
            _tagRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Tag>())).Returns(Task.FromResult(tag));

            // Act: when we create it
            tag = await _blogSvc.CreateTagAsync(tag);

            // Assert: then it has the expected slug
            Assert.Equal(expectSlug, tag.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateTagAsync(Tag)"/> with user input both title and slug.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="slug"></param>
        /// <param name="expectSlug"></param>
        [Theory]
        [InlineData("Web Development!", "whatever-i-want", "whatever-i-want")]
        [InlineData("Tech", "tech", "tech-2")]
        public async void CreateTag_User_Input_Title_And_Slug(string title, string slug, string expectSlug)
        {
            // Arrange: a category and a repo to return it
            var tag = new Tag { Title = title, Slug = slug };
            _tagRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Tag>())).Returns(Task.FromResult(tag));

            // Act: when we create it
            tag = await _blogSvc.CreateTagAsync(tag);

            // Assert: then it has the expected slug
            Assert.Equal(expectSlug, tag.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateTagAsync(Tag)"/> throws excpetion if title exists already.
        /// </summary>
        [Fact]
        public async void CreateTag_Throws_FanException_If_Title_Already_Exist()
        {
            // Arrange: a category with a title that exists
            var tag = new Tag { Title = "Technology" };

            // Act and Assert: when we create it, we get exception
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.CreateTagAsync(tag));

            // Act and Assert: error message
            try
            {
                await _blogSvc.CreateTagAsync(tag);
            }
            catch (FanException ex)
            {
                Assert.Equal("Failed to create Tag.", ex.Message);
                Assert.Equal(1, ex.ValidationFailures.Count);
                Assert.Equal("Tag 'Technology' is not available, please choose a different one.", ex.ValidationFailures[0].ErrorMessage);
            }
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateTagAsync(Tag)"/> a title in Chinese with no manual slug yields a 6-char random string.
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
            tag = await _blogSvc.CreateTagAsync(tag);

            // Assert: then it has the expected slug length
            Assert.Equal(6, tag.Slug.Length);
        }

        /// <summary>
        /// Test <see cref="BlogService.CreateTagAsync(Tag)"/> would call TagRepository's CreateAsync and invalidates cache for all tags.
        /// </summary>
        [Fact]
        public async void CreateTag_Calls_TagRepository_CreateAsync_And_Invalidates_Cache_For_AllTags()
        {
            // Arrange 
            var tag = new Tag { Title = "Tag1" };

            // Act
            await _blogSvc.CreateTagAsync(tag);

            // Assert
            _tagRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Tag>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_TAGS));
        }

        /// <summary>
        /// Test <see cref="BlogService.DeleteTagAsync(int)"/> calls TagRepository Delete method.
        /// </summary>
        [Fact]
        public async void DeleteTag_Calls_TagRepository_DeleteAsync_And_Invalidates_Cache_For_AllTags()
        {
            // Act
            await _blogSvc.DeleteTagAsync(1);

            // Assert
            _tagRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_TAGS));
        }

        /// <summary>
        /// Test <see cref="BlogService.GetTagAsync(string)"/> either by id or by slug throws exception if not found.
        /// </summary>
        [Fact]
        public async void GetTag_Throws_FanException_If_Not_Found()
        {
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.GetTagAsync(100));
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.GetTagAsync("slug-not-exist"));
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateTagAsync(Tag)"/> updates a tag with new title and slug.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <param name="newSlug"></param>
        /// <param name="expectedTitle"></param>
        /// <param name="expectedSlug"></param>
        [Theory]
        [InlineData("Web Development!", "whatever-i-want", "Web Development!", "whatever-i-want")]
        [InlineData("Tech", "tech", "Tech", "tech-2")]
        public async void UpdateTag_Updates_New_Title_And_Slug(string newTitle, string newSlug, string expectedTitle, string expectedSlug)
        {
            // Arrange: a tag
            var tag = new Tag { Title = newTitle, Slug = newSlug };
            _tagRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Tag>())).Returns(Task.FromResult(tag));

            // Act: update
            tag = await _blogSvc.UpdateTagAsync(tag);

            // Assert
            Assert.Equal(expectedTitle, tag.Title);
            Assert.Equal(expectedSlug, tag.Slug);
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateTagAsync(Tag)"/> throws excpetion if title exists already.
        /// </summary>
        [Fact]
        public async void UpdateTag_Throws_FanException_If_Title_Already_Exist()
        {
            // Arrange: a tag with a title that exists
            var tag = new Tag { Title = "Technology" };

            // Act and Assert: when we create it, we get exception
            await Assert.ThrowsAsync<FanException>(() => _blogSvc.UpdateTagAsync(tag));

            // Act and Assert: error message
            try
            {
                await _blogSvc.UpdateTagAsync(tag);
            }
            catch (FanException ex)
            {
                Assert.Equal("Failed to update Tag.", ex.Message);
                Assert.Equal(1, ex.ValidationFailures.Count);
                Assert.Equal("Tag 'Technology' is not available, please choose a different one.", ex.ValidationFailures[0].ErrorMessage);
            }
        }

        /// <summary>
        /// Test <see cref="BlogService.UpdateTagAsync(Tag)"/> would call TagRepository's UpdateAsync and invalidates cache for all tags.
        /// </summary>
        [Fact]
        public async void UpdateTag_Calls_TagRepository_And_Invalidates_Cache_For_AllTags()
        {
            // Arrange 
            var tag = new Tag { Title = "Tag1" };

            // Act
            await _blogSvc.UpdateTagAsync(tag);

            // Assert
            _tagRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Exactly(1));
            Assert.Null(await _cache.GetAsync(BlogService.CACHE_KEY_ALL_TAGS));
        }
    }
}
