using Fan.Helpers;
using Fan.Settings;
using Humanizer;
using System;
using System.Net;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    public class UtilTest
    {
        /// <summary>
        /// Test for <see cref="Util.Slugify(string)"/>.
        /// </summary>
        [Theory]
        [InlineData("c#", "c")]
        [InlineData(" It'll rain , tomorrow-", "itll-rain-tomorrow")]
        [InlineData("my 1.0 release", "my-1-0-release")]
        [InlineData("1+1=2", "11-2")]
        [InlineData("1.1-1_1_.-", "1-1-1-1")]
        [InlineData("你好。", "")]
        [InlineData("<script>A post title</script>", "scripta-post-title-script")]
        public void Slugify_turns_a_string_into_url_friendly_slug(string title, string expected)
        {
            Assert.Equal(expected, Util.Slugify(title));
        }

        /// <summary>
        /// Test for <see cref="Util.GetExcerpt(string, int)"/> method.
        /// TODO should have a performance test on GetExcerpt.
        /// </summary>
        [Theory]
        [InlineData("<p>A body less than 55 words</p>", "A body less than 55 words", 55)]
        [InlineData("<p>A body more than 5 words.</p>", "A body more than 5 words.", 6)]
        [InlineData("<p>A body more than 5 words.</p>", "A body more than 5…", 5)]
        [InlineData("<p></p>", "", 55)]
        public void GetExcerpt_returns_a_string_excerpt_from_a_given_html(string body, string expected, int wordLimit)
        {
            Assert.Equal(expected, Util.GetExcerpt(body, wordLimit));
        }

        /// <summary>
        /// Test for <see cref="Util.ConvertTime(DateTimeOffset, string)"/>.
        /// </summary>
        [Fact]
        public void ConvertTime_Converts_UtcTime_To_A_Specified_Timezone()
        {
            // suppose a site owner lives in US west coast
            // so he sets the site with the following timezone
            var coreSettings = new CoreSettings
            {
                TimeZoneId = "Pacific Standard Time"
            };

            // he published a post at his local time 2017/10/14 16:22:00,
            // since server saves all posts with DateTimeOffset.UtcNow, so it
            // records this time for the post CreatedOn
            var createdOn = new DateTimeOffset(2017, 10, 14, 23, 22, 0, TimeSpan.Zero);

            // the user wants to see the actual post time in his own timezone
            // Util.ConvertTime returns him that
            var displayToUser = Util.ConvertTime(createdOn, coreSettings.TimeZoneId);
            // https://en.wikipedia.org/wiki/Pacific_Time_Zone
            // Pacific Time Zone observes standard time by subtracting eight hours from Coordinated Universal Time (UTC−8). 
            // During daylight saving time, a time offset of UTC−7 is used.
            Assert.Equal("-07:00:00", displayToUser.Offset.ToString());
        }

        /// <summary>
        /// DateTimeOffset is used throughout the system as recommended 
        /// <see cref="https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime"/>
        /// DateTime is ambiguous wheras DateTimeOffset has an offset relating to UTC making it 
        /// very clear spot in time.
        /// </summary>
        [Fact]
        public void DateTimeOffset_should_be_used_instead_of_DateTime()
        {
            Assert.Equal("now", DateTimeOffset.UtcNow.Humanize());
            Assert.Equal("now", DateTimeOffset.Now.Humanize());

            // OK
            Assert.Equal("now", DateTime.UtcNow.Humanize());
            // Ambiguous
            // Fails on IANA time zone machine it outputs "now" 
            // but on my local windows it outputs "7 hours ago"
            // Assert.NotEqual("now", DateTime.Now.Humanize()); 
        }

        [Theory]
        [InlineData("你好。", "你好。")]
        [InlineData("c#", "c#")]
        [InlineData("<h1>", "&lt;h1&gt;")]
        public void WebUtility_HtmlEncode_escapes_special_chars(string input, string expected)
        {
            Assert.Equal(expected, WebUtility.HtmlEncode(input));
        }

        /// <summary>
        /// Test <see cref="Uri"/> class, I use this class as a parser and it's important to 
        /// understand what it does.
        /// </summary>
        [Fact]
        public void TestUri()
        {
            Uri uri = new Uri("http://test.com/about?name=john&age=10#abc");
            Assert.Equal("/about", uri.AbsolutePath);
            Assert.Equal("http://test.com/about?name=john&age=10#abc", uri.AbsoluteUri);
            Assert.Equal("test.com", uri.Authority);
            Assert.Equal("test.com", uri.DnsSafeHost);
            Assert.Equal("#abc", uri.Fragment);
            Assert.Equal("test.com", uri.Host);
            Assert.Equal("http://test.com/about?name=john&age=10#abc", uri.OriginalString);
            Assert.Equal("/about?name=john&age=10", uri.PathAndQuery);
            Assert.Equal(80, uri.Port);
            Assert.Equal("?name=john&age=10", uri.Query);
            Assert.Equal("http", uri.Scheme);
            Assert.Equal(2, uri.Segments.Length);
        }

        [Theory]
        [InlineData("<h1>Hello</h1>", "Hello")]
        [InlineData("<img src=\"mypic.jpg\">", "")]
        public void CleanHtml_removes_all_html_tags(string content, string expected)
        {
            Assert.Equal(expected, Util.CleanHtml(content));
        }

        /// <summary>
        /// Test cases for <see cref="RegexUtilities.IsValidEmail(string)"/> method.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="expected"></param>
        /// <remarks>
        /// Test data provided by https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format#compiling-the-code
        /// </remarks>
        [Theory]
        [InlineData("david.jones@proseware.com", true)]
        [InlineData("d.j@server1.proseware.com", true)]
        [InlineData("jones@ms1.proseware.com", true)]
        [InlineData("j.@server1.proseware.com", false)]
        [InlineData("j@proseware.com9", true)]
        [InlineData("js#internal@proseware.com", true)]
        [InlineData("j_9@[129.126.118.1]", true)]
        [InlineData("j..s@proseware.com", false)]
        [InlineData("js*@proseware.com", false)]
        [InlineData("js@proseware..com", false)]
        [InlineData("js@proseware.com9", true)]
        [InlineData("j.s@server1.proseware.com", true)]
        [InlineData(@"""j\""s\""""@proseware.com", true)]
        [InlineData("js@contoso.中国", true)]
        [InlineData("a@a", false)]
        public void IsValidEmail_Test(string email, bool expected)
        {
            Assert.Equal(expected, Util.IsValidEmail(email));
        }

        [Fact]
        public void IsRunningFromTestHost_Test()
        {
            Assert.True(Util.IsRunningFromTestHost());
        }
    }
}
