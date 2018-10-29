using Fan.Blog.IntegrationTests.Base;
using Fan.Blog.IntegrationTests.Helpers;
using Fan.Medias;
using Moq;
using System.IO;
using Xunit;

namespace Fan.Blog.IntegrationTests
{
    /// <summary>
    /// Test cases for when an author uploads an image either from browser or OLW.
    /// </summary>
    /// <remarks>
    /// <see cref="MetaWeblogServiceTest"/> for upload image from OLW test.
    /// </remarks>
    public class ImageServiceTest : BlogServiceIntegrationTestBase
    {
        /// <summary>
        /// A 40 x 40 png image in base64.
        /// </summary>
        const string IMAGE_BASE64 = "iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAYAAACM/rhtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAN8SURBVFhHzZjNS1RRFMDvuTOOlQsNIsiyEGoVrgoCRWQQMtRwCoKgNhNEf0DRpkWLolUFbmZRayNaaJRCLiIsjILaOEQLJbMS0T7IZgydmXdv59x737w31qjvQ3q/4cw959773jvvnPs1A7NnmyeA8RZGSBRQmoZswq6rYltM9DYNTI9oS3N9rL2HMxgG1Re/1L1dD5Co60aNskmhL91PSpHlpEtsVEIfW7dtd10Vey2wm9O34lkuvWzbdbokYPZM8wQWLVjzlHH+UNV6BIT1uPHexxljKq6NJ/fFSqXjxvQIpDCAnehTtuygYOxW08CHS6bHf+XGWPtNxvhFxijFUcUMz+g6SOMQC66HYhTRnnHXRI8c5Ft0U2yIbooxfGoMRjbF6B355nsdLAzu6JCC7zbmmkBcfEmkvj035rrodRBwHZRZ3ymWjF+GGBsgYaasJlKA9w0A9+nAKaYbuF/Qtu06R/fxFLyQrgowSZwrcWP/jVv8AmplcdvY86fp6gF9gAgQQedK1O4kmvbuqSa12xfOma4egKARdAFMwOG3xaqSZCXTc+OY9+dc8jTIWEeNjGV0lQ9wxsg3h2oq5Kr/TQC4zIAUHVimfWd4ZXDnCE60btLxbDmHxTTpDtBfe3L+gTF8E2iro+GhhgiwXSittqDjrRJkIzUFJZCDFP5/SZj4vt+qFL/ESI6qBoPFYbQuNf/amL6B86+Gk2CJ+nhcTmaO9L0z9etCDuLrddMb4op1u/bEV9yawqHrxdxB/EV3gAm+yLkl+jESQ0UL0qbdE6EsU6soiVJaSDZkgeg3Y9BvpnExDXvQGei25Jx2MMBDzM/X0LFvqx30+xD6p2ATI4hHwAARxO0No2fhy5FsShydFPsg0bD1VGIb1NeQbKm/YqpDx6QYN1OlbBxIzizzrvklJd1TK6Y6dPRxC6ci5TuK6OMWfvnO9Saj/cIwRimC7mBFMnAULHtZ0A5GNMXOOohEcZKQc5FMsQ0FDS6MP5rAxaZFCjmO9hPV4pE4yPuZ1r4pYyqOPvu03+L8tDE9gXP2GB5D2lDNlh3UTdWh9VL9sU3HF7skUBXS6r3blqr4l79z7HMPBz781yaobqTVCt2msi7Ll3/lYoVcnhVySyhUusWpW6EyTzaKKk2ffJ79mJypw5s1uGVp6n1dMbfIivlV4q5bp72w+D32B8Tg4wsSF0ucAAAAAElFTkSuQmCC";

        private readonly byte[] _sourceArray;
        private readonly Stream _sourceStream;

        /// <summary>
        /// Initializes an image stream and byte array.
        /// </summary>
        public ImageServiceTest()
        {
            _sourceArray = System.Convert.FromBase64String(IMAGE_BASE64);
            _sourceStream = new MemoryStream(_sourceArray);
        }

        /// <summary>
        /// When an author uploads an image from either composer "admin/compose" or gallery "admin/media".
        /// </summary>
        /// <remarks>
        /// From composer or gallery, image is stream
        /// </remarks>
        [Fact]
        public async void Author_can_upload_images_from_Composer_or_MediaGallery()
        {
            // Given an existing image
            string contentType = "image/png";
            string filename = "fanray logo.png";
            string filenameSlugged = "fanray-logo.png";
            SeedImages(filenameSlugged);

            // When user uploads an image with the same name
            var media = await _imgSvc.UploadAsync(_sourceStream, Actor.ADMIN_ID, filename, contentType, EUploadedFrom.Browser);

            // Then a second record is inserted
            Assert.Equal(2, media.Id);

            // content type matches
            Assert.Equal(contentType, media.ContentType);

            // with a unique name
            Assert.Equal("fanray-logo-1.png", media.FileName);

            // 0 resized, only orig was saved
            Assert.Equal(0, media.ResizeCount);

            // title, caption and alt
            Assert.Equal("fanray logo", media.Title);
            Assert.Equal("fanray logo", media.Caption);
            Assert.Equal("fanray logo", media.Alt);

            // and storage provider is called only once since it's a tiny image
            _storageProviderMock.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<char>()),
                Times.Exactly(1));
        }
    }
}
