using Fan.Settings;
using Fan.Web.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Fan.Web.Tests.UrlRewrite
{
    /// <summary>
    /// <see cref="PreferredDomainRewriter"/> tests.
    /// </summary>
    public class PreferredDomainRewriterTest
    {
        private IPreferredDomainRewriter _rewriter;
        public PreferredDomainRewriterTest()
        {
            _rewriter = new PreferredDomainRewriter();
        }

        public static List<object[]> Requests
        {
            get
            {
                var scheme = "https";

                var _request1 = new Mock<HttpRequest>();
                _request1.Setup(r => r.Scheme).Returns(scheme);
                _request1.Setup(r => r.Host).Returns(new HostString("www.test.com"));

                var _request2 = new Mock<HttpRequest>();
                _request2.Setup(r => r.Scheme).Returns(scheme);
                _request2.Setup(r => r.Host).Returns(new HostString("test.com"));

                var _request3 = new Mock<HttpRequest>();
                _request3.Setup(r => r.Scheme).Returns(scheme);
                _request3.Setup(r => r.Host).Returns(new HostString("blog.test.com"));

                var data = new List<object[]>
                {
                    new object[] { _request1, EPreferredDomain.Www, null },
                    new object[] { _request2, EPreferredDomain.Www, "https://www.test.com" },
                    new object[] { _request1, EPreferredDomain.NonWww, "https://test.com" },
                    new object[] { _request2, EPreferredDomain.NonWww, null },

                    new object[] { _request1, EPreferredDomain.Auto, null },
                    new object[] { _request3, EPreferredDomain.Www, null },
                    new object[] { _request3, EPreferredDomain.NonWww, null },
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Requests))]
        public void RewriteTest(Mock<HttpRequest> request, EPreferredDomain preferredDomain, string expectedUrl)
        {
            string url = _rewriter.Rewrite(request.Object, preferredDomain);
            Assert.Equal(expectedUrl, url);
        }
    }
}