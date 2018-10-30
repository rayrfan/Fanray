using Fan.Helpers;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="Fan.Helpers.OembedParser"/>.
    /// </summary>
    public class OembedParserTest
    {
        [Theory]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U")]
        [InlineData("https://youtu.be/MNor4dYXa6U")]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U&w=800&h=400&start=75")]
        public void GetOembedType_returns_type_YouTube_given_any_youtube_urls(string url)
        {
            Assert.Equal(EEmbedType.YouTube, OembedParser.GetOembedType(url));
        }

        [Fact]
        public void GetOembedType_returns_type_Vimeo_given_vimeo_url()
        {
            Assert.Equal(EEmbedType.Vimeo, OembedParser.GetOembedType("https://vimeo.com/1084537"));
        }

        //[Fact]
        //public void GetOembedType_returns_type_Twitter_given_a_tweet_url()
        //{
        //    Assert.Equal(EEmbedType.Twitter, OembedParser.GetOembedType("https://twitter.com/fanraymedia/status/1050859415430033408"));
        //}

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U&w=800&h=400&start=75", "MNor4dYXa6U&w=800&h=400&start=75")]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U&t=762s", "MNor4dYXa6U&start=762s")]
        [InlineData("https://www.youtube.com/embed/MNor4dYXa6U&amp;t=726s", "MNor4dYXa6U&amp;start=726s")]
        [InlineData("https://youtu.be/MNor4dYXa6U", "MNor4dYXa6U")]
        public void GetYouTubeVideoKey_gets_key_from_url(string url, string expected)
        {
            Assert.Equal(expected, OembedParser.GetYouTubeVideoKey(url));
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U")]
        [InlineData("https://www.youtube.com/embed/MNor4dYXa6U")]
        [InlineData("https://youtu.be/MNor4dYXa6U")]
        public void GetYouTubeEmbed_returns_youtube_embed_html(string url)
        {
            string expected = @"<iframe width=""800"" height=""450"" src =""https://www.youtube.com/embed/MNor4dYXa6U"" frameborder =""0"" allow=""autoplay; encrypted - media"" allowfullscreen></iframe>";
            Assert.Equal(expected, OembedParser.GetYouTubeEmbed(url));
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=MNor4dYXa6U&w=800&h=400&start=75", 800, 400, 75)]
        public void GetYouTubeEmbed_returns_youtube_embed_html_with_size_and_start_info(string url, int width, int height, int start)
        {
            string expected = $@"<iframe width=""{width}"" height=""{height}"" src =""https://www.youtube.com/embed/MNor4dYXa6U?start={start}"" frameborder =""0"" allow=""autoplay; encrypted - media"" allowfullscreen></iframe>";
            Assert.Equal(expected, OembedParser.GetYouTubeEmbed(url));
        }

        [Theory]
        [InlineData("https://vimeo.com/1084537")]
        public void GetVimeoEmbed_returns_vimeo_embed_html(string url)
        {
            string expected = @"<iframe width=""800"" height=""450"" src =""https://player.vimeo.com/video/1084537"" frameborder =""0"" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
            Assert.Equal(expected, OembedParser.GetVimeoEmbed(url));
        }

        [Theory]
        [InlineData(@"<figure class=""media""><oembed url=""https://www.youtube.com/watch?v=MNor4dYXa6U""></oembed></figure>",
            @"<figure class=""media""><iframe width=""800"" height=""450"" src=""https://www.youtube.com/embed/MNor4dYXa6U"" frameborder=""0"" allow=""autoplay; encrypted - media"" allowfullscreen=""""></iframe></figure>")]
        [InlineData(@"<figure class=""media""><oembed url=""https://vimeo.com/1084537""></oembed></figure>",
            @"<figure class=""media""><iframe width=""800"" height=""450"" src=""https://player.vimeo.com/video/1084537"" frameborder=""0"" webkitallowfullscreen="""" mozallowfullscreen="""" allowfullscreen=""""></iframe></figure>")]
        public void Parse_returns_body_with_proper_html_embed(string body, string expected)
        {
            Assert.Equal(expected, OembedParser.Parse(body));
        }
    }
}
