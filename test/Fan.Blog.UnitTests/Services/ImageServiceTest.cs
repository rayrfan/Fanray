using Fan.Blog.Enums;
using Fan.Blog.UnitTests.Base;
using Fan.Medias;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ImageService"/>.
    /// </summary>
    public class ImageServiceTest : BlogServiceUnitTestBase
    {
        readonly string _absPath;
        readonly Media _media;
        const string FILENAME = "pic.jpg";
        const string STORAGE_ENDPOINT = "https://localhost:44381";

        /// <summary>
        /// Consturctor initialization called before each test.
        /// </summary>
        public ImageServiceTest()
        {
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year;
            var month = uploadedOn.Month.ToString("d2");
            _absPath = $"{STORAGE_ENDPOINT}/media/blog/{year}/{month}";

            _storageProMock.Setup(pro => pro.StorageEndpoint).Returns(STORAGE_ENDPOINT);

            _media = new Media
            {
                FileName = FILENAME,
                UploadedOn = uploadedOn,
            };
        }

        [Fact]
        public async void ProcessResponsiveImageAsync_on_large_2200x1650_landscape_picture()
        {
            // Setup media
            _mediaSvcMock.Setup(svc => svc.GetMediaAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Media
                {
                    FileName = "painting-2200x1650.jpg",
                    ResizeCount = 4,
                    Width = 2200,
                    Height = 1650,
                    UploadedOn = new DateTimeOffset(2019, 4, 3, 0, 0, 0, TimeSpan.Zero),
                }));

            var input = "<img src=\"https://localhost:44381/media/blog/2019/04/md/painting-2200x1650.jpg\" alt=\"painting 2200x1650\">";
            var expected = "<img src=\"https://localhost:44381/media/blog/2019/04/md/painting-2200x1650.jpg\" alt=\"painting 2200x1650\" " +
                           "srcset=\"https://localhost:44381/media/blog/2019/04/sm/painting-2200x1650.jpg 400w, " +
                           "https://localhost:44381/media/blog/2019/04/md/painting-2200x1650.jpg 800w, " + 
                           "https://localhost:44381/media/blog/2019/04/ml/painting-2200x1650.jpg 2x, " +
                           "https://localhost:44381/media/blog/2019/04/lg/painting-2200x1650.jpg 3x\" " +
                           "sizes=\"(max-width: 1200px) 100vw, 1200px\">";
            var output = await _imgSvc.ProcessResponsiveImageAsync(input);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async void ProcessRepsonsiveImageAsync_on_medium_large_960x1440_portrait_picture()
        {
            // Setup media
            _mediaSvcMock.Setup(svc => svc.GetMediaAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Media
                {
                    FileName = "nightsky-960x1440.jpg",
                    ResizeCount = 3,
                    Width = 960,
                    Height = 1440,
                    UploadedOn = new DateTimeOffset(2019, 4, 3, 0, 0, 0, TimeSpan.Zero),
                }));

            var input = "<img src=\"https://localhost:44381/media/blog/2019/04/md/nightsky-960x1440.jpg\" alt=\"nightsky 960x1440\">";
            var expected = "<img src=\"https://localhost:44381/media/blog/2019/04/md/nightsky-960x1440.jpg\" alt=\"nightsky 960x1440\" "+
                           "srcset=\"https://localhost:44381/media/blog/2019/04/sm/nightsky-960x1440.jpg 400w, "+
                           "https://localhost:44381/media/blog/2019/04/md/nightsky-960x1440.jpg 800w, "+
                           "https://localhost:44381/media/blog/2019/04/ml/nightsky-960x1440.jpg 2x, "+
                           "https://localhost:44381/media/blog/2019/04/nightsky-960x1440.jpg 3x\" "+
                           "sizes=\"(max-width: 960px) 100vw, 960px\">";
            var output = await _imgSvc.ProcessResponsiveImageAsync(input);

            Assert.Equal(expected, output);
        }

        [Fact]
        public async void ProcessRepsonsiveImageAsync_on_tiny_90x90_square_picture()
        {
            // Setup media
            _mediaSvcMock.Setup(svc => svc.GetMediaAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Media
                {
                    FileName = "sq-90x90.png",
                    ResizeCount = 0,
                    Width = 90,
                    Height = 90,
                    UploadedOn = new DateTimeOffset(2019, 4, 3, 0, 0, 0, TimeSpan.Zero),
                }));

            var input = "<img src=\"https://localhost:44381/media/blog/2019/04/sq-90x90.png\" alt=\"sq 90x90\">";
            var expected = "<img src=\"https://localhost:44381/media/blog/2019/04/sq-90x90.png\" alt=\"sq 90x90\">";
            var output = await _imgSvc.ProcessResponsiveImageAsync(input);

            Assert.Equal(expected, output);
        }

        /// <summary>
        /// When there is no resize, you will always get original image url.
        /// </summary>
        [Fact]
        public void GetImageUrl_with_0_ResizeCount()
        {
            // Given a media with no resize count
            _media.ResizeCount = 0;

            // Regardless which size you ask it'll return original
            var origUrl = $"{_absPath}/{FILENAME}";

            // original -> original
            var actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Medium);
            Assert.Equal(origUrl, actualUrl);

            // small -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Small);
            Assert.Equal(origUrl, actualUrl);
        }

        /// <summary>
        /// When there is 1 resize, meaning only original and small were saved,
        /// unless you asked for small, you'll always get original.
        /// </summary>
        /// <remarks>
        /// Image author uploads an image of width 450px, the cut off for small is 400px,
        /// thus it will save original 450px and one small 400px.  When asked for a large,
        /// it will return you the original 450px image.
        /// </remarks>
        [Fact]
        public void GetImageUrl_with_1_ResizeCount()
        {
            // Given a media with 1 resize count
            _media.ResizeCount = 1;

            // You will get small unless you ask for original
            var origUrl = $"{_absPath}/{FILENAME}";

            // original -> original
            var actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Medium);
            Assert.Equal(origUrl, actualUrl);

            // small -> small
            var smallUrl = $"{_absPath}/sm/{FILENAME}";
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Small);
            Assert.Equal(smallUrl, actualUrl);
        }

        /// <summary>
        /// When there is 2 resizes, meaning original, small, medium have been saved,
        /// you'll get original only if you ask for orginal and large, in all other cases you get
        /// what you ask for.
        /// </summary>
        [Fact]
        public void GetImageUrl_with_2_ResizeCount()
        {
            // Given a media with 2 resize counts
            _media.ResizeCount = 2;

            var origUrl = $"{_absPath}/{FILENAME}";
            var smallUrl = $"{_absPath}/sm/{FILENAME}";
            var mediumUrl = $"{_absPath}/md/{FILENAME}";

            // original -> original
            var actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> medium
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Medium);
            Assert.Equal(mediumUrl, actualUrl);

            // small -> small
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Small);
            Assert.Equal(smallUrl, actualUrl);
        }

        /// <summary>
        /// When there is 3 resizes, unless you asked for original, you get small.
        /// </summary>
        [Fact]
        public void GetImageUrl_with_3_ResizeCount()
        {
            _media.ResizeCount = 3;

            var origUrl = $"{_absPath}/{FILENAME}";
            var smallUrl = $"{_absPath}/sm/{FILENAME}";
            var mediumUrl = $"{_absPath}/md/{FILENAME}";
            var largeUrl = $"{_absPath}/lg/{FILENAME}";

            // original -> original
            var actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> large
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Large);
            Assert.Equal(largeUrl, actualUrl);

            // medium -> medium
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Medium);
            Assert.Equal(mediumUrl, actualUrl);

            // small -> small
            actualUrl = _imgSvc.GetAbsoluteUrl(_media, EImageSize.Small);
            Assert.Equal(smallUrl, actualUrl);
        }
    }
}
