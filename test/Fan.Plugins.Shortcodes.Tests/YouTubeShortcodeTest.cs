using Xunit;

namespace Fan.Plugins.Shortcodes.Tests
{
    /// <summary>
    /// Tests for <see cref="YouTubeShortcode"/> class.
    /// </summary>
    /// <remarks>
    /// For a list of possible youtube url formats <see cref="https://stackoverflow.com/a/10315969/32240"/>
    /// and <see cref="https://stackoverflow.com/a/37704433/32240"/>
    /// </remarks>
    public class YouTubeShortcodeTest
    {
        private IShortcodeService _svc;
        public YouTubeShortcodeTest()
        {
            _svc = new ShortcodeService();
            _svc.Add<YouTubeShortcode>(tag: "youtube");
        }

        /// <summary>
        /// Currently I only support 2 kinds of yt url formats.
        /// </summary>
        /// <param name="url"></param>
        [Theory]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U")]
        [InlineData("https://youtu.be/MNor4dYXa6U")]
        [InlineData("www.youtube.com/watch?v=MNor4dYXa6U")]
        [InlineData("youtu.be/MNor4dYXa6U")]
        public void YouTubeShortcode_supports_two_kinds_of_youtube_url_formats(string url)
        {
            var result = _svc.Parse($"[youtube {url}]");
            Assert.Equal(
                "<iframe width=\"560\" height=\"315\" src =\"https://www.youtube.com/embed/MNor4dYXa6U\" frameborder =\"0\" allowfullscreen></iframe>",
                result);
        }
    }
}
