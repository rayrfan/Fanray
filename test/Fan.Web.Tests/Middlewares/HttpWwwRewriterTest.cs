using Fan.Settings;
using Fan.Web.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Fan.Web.Tests.UrlRewrite
{
    public class AppSettingsTestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = true },
                "http://test.com/about?name=john&age=10",
                true,
                "https://www.test.com/about?name=john&age=10"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = true },
                "https://www.test.com/about?name=john",
                false, // when the url is already correct, I don't do redirect
                "https://www.test.com/about?name=john"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = false },
                "http://test.com/about?name=john",
                true,
                "http://www.test.com/about?name=john"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = false }, // when UseHttps is set to false
                "https://www.test.com/about?name=john", // but user is using https, the recommended way
                false, // I don't do redirect
                "https://www.test.com/about?name=john"  // I don't downgrade you
            },
            // --------------------------------------------------- nonwww tests
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.NonWww, UseHttps = true },
                "http://www.test.com/about?name=john",
                true,
                "https://test.com/about?name=john"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.NonWww, UseHttps = true },
                "https://test.com/about?name=john",
                false, // when the url is already correct, we don't do redirect
                "https://test.com/about?name=john"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.NonWww, UseHttps = false },
                "http://www.test.com/about?name=john",
                true,
                "http://test.com/about?name=john"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.NonWww, UseHttps = false }, // when UseHttps is set to false
                "https://test.com/about?name=john", // but user is using https, the recommended way
                false, // I don't do redirect
                "https://test.com/about?name=john"  // I don't downgrade you
            },
            // -------------------------------------------------- www / nonwww setting makes no effect if subdomain is other than www
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = true },
                "http://plus.google.com/+John/posts/BPn85MDmCSL",
                true,
                "https://plus.google.com/+John/posts/BPn85MDmCSL"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Www, UseHttps = true },
                "https://plus.google.com/+John/posts/BPn85MDmCSL",
                false,
                "https://plus.google.com/+John/posts/BPn85MDmCSL"
            },
            new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.NonWww, UseHttps = true },
                "http://blog.test.com/#resource/sub/af-6f/res/ft.eb/site",
                true,
                "https://blog.test.com/#resource/sub/af-6f/res/ft.eb/site",
            },
            // --------------------------------------------------- auto
             new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Auto, UseHttps = true },
                "http://plus.google.com/+John/posts/BPn85MDmCSL",
                true,
                "https://plus.google.com/+John/posts/BPn85MDmCSL"
            },
             new object[] {
                new AppSettings { PreferredDomain = EPreferredDomain.Auto, UseHttps = false },
                "http://plus.google.com/+John/posts/BPn85MDmCSL",
                false,
                "http://plus.google.com/+John/posts/BPn85MDmCSL"
            },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class HttpWwwRewriterTest
    {
        private IHttpWwwRewriter _helper;
        public HttpWwwRewriterTest()
        {
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _helper = new HttpWwwRewriter(loggerFactory.CreateLogger<HttpWwwRewriter>());
        }

        [Theory]
        [ClassData(typeof(AppSettingsTestData))]
        public void ShouldRewriteTest(AppSettings appSettings, string requestUrl, bool expectedShould, string expectedUrl)
        {
            bool should = _helper.ShouldRewrite(appSettings, requestUrl, out string url);
            Assert.Equal(expectedShould, should);
            Assert.Equal(expectedUrl, url);
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
    }
}