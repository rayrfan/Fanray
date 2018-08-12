using Fan.Blog.Enums;
using Fan.Blog.UnitTests.Base;
using Fan.Medias;
using System;
using Xunit;

namespace Fan.Blog.UnitTests.Services
{
    public class ImageTest : BlogServiceUnitTestBase
    {
        const string FILENAME = "pic.jpg";
        readonly string path;
        readonly Media _media;

        /// <summary>
        /// Consturctor initialization called before each test.
        /// </summary>
        public ImageTest()
        {
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year;
            var month = uploadedOn.Month.ToString("d2");
            path = $"{STORAGE_ENDPOINT}/media/blog/{year}/{month}";

            _media = new Media
            {
                FileName = FILENAME,
                UploadedOn = uploadedOn,
            };
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
            var origUrl = $"{path}/{FILENAME}";

            // original -> original
            var actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Medium);
            Assert.Equal(origUrl, actualUrl);

            // small -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Small);
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
            var origUrl = $"{path}/{FILENAME}";

            // original -> original
            var actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Medium);
            Assert.Equal(origUrl, actualUrl);

            // small -> small
            var smallUrl = $"{path}/sm/{FILENAME}";
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Small);
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

            var origUrl = $"{path}/{FILENAME}";
            var smallUrl = $"{path}/sm/{FILENAME}";
            var mediumUrl = $"{path}/md/{FILENAME}";

            // original -> original
            var actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> original
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Large);
            Assert.Equal(origUrl, actualUrl);

            // medium -> medium
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Medium);
            Assert.Equal(mediumUrl, actualUrl);

            // small -> small
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Small);
            Assert.Equal(smallUrl, actualUrl);
        }

        /// <summary>
        /// When there is 3 resizes, unless you asked for original, you get small.
        /// </summary>
        [Fact]
        public void GetImageUrl_with_3_ResizeCount()
        {
            _media.ResizeCount = 3;

            var origUrl = $"{path}/{FILENAME}";
            var smallUrl = $"{path}/sm/{FILENAME}";
            var mediumUrl = $"{path}/md/{FILENAME}";
            var largeUrl = $"{path}/lg/{FILENAME}";

            // original -> original
            var actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Original);
            Assert.Equal(origUrl, actualUrl);

            // large -> large
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Large);
            Assert.Equal(largeUrl, actualUrl);

            // medium -> medium
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Medium);
            Assert.Equal(mediumUrl, actualUrl);

            // small -> small
            actualUrl = _blogSvc.GetImageUrl(_media, EImageSize.Small);
            Assert.Equal(smallUrl, actualUrl);
        }
    }
}
