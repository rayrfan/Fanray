using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blogs.Tests.Services
{
    public class MediaTest : BlogUnitTestBase
    {
        /// <summary>
        /// Test <see cref="BlogService.CreateMediaAsync(Media)"/> would call PostRepository's CreateAsync.
        /// </summary>
        [Fact]
        public async void UpsertMedia_Calls_PostRepository_CreateAsync()
        {
            // Arrange 
            var media = new Media { Title = "File1", Slug = "file1" };

            // Act
            await _blogSvc.UpsertMediaAsync(media);

            // Assert
            _postRepoMock.Verify(repo => repo.CreateAsync(It.IsAny<Post>()), Times.Exactly(1));
        }

        /// <summary>
        /// When user uploads a file with same name, it'll update it.
        /// </summary>
        [Fact]
        public async void UpsertMedia_Updates_Media_If_It_Exists_Already()
        {
            // Arrange an existing file
            _postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), EPostType.Media)).Returns(Task.FromResult(new Post { Title = "image1.jpg" }));

            // Act: adding a file with existing name
            await _blogSvc.UpsertMediaAsync(new Media { Title = "image1.jpg" });

            // Assert: will update it
            _postRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Exactly(1));
        }

        /// <summary>
        /// Test <see cref="BlogService.GetMediaAsync(string)"/> returns null if slug is not found.
        /// </summary>
        [Fact]
        public async void GetMedia_Returns_Null_If_Slug_Not_Found()
        {
            var media = await _blogSvc.GetMediaAsync("not-exist");

            Assert.Null(media);
        }

        /// <summary>
        /// Test <see cref="BlogService.GetMediaAsync(string)"/> calls PostRepository's GetAsync.
        /// </summary>
        [Fact]
        public async void GetMedia_Calls_PostRepository_GetAsync()
        {
            // Arrange 
            _postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), EPostType.Media)).Returns(Task.FromResult(new Post { Title = "image1.jpg" }));

            // Act
            var media = await _blogSvc.GetMediaAsync(It.IsAny<string>());

            // Assert
            _postRepoMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), EPostType.Media), Times.Exactly(1));
            Assert.True(media is Media);
        }
    }
}
