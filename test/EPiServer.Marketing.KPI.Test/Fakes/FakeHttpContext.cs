using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.KPI.Test.Fakes
{
    /// <summary>
    /// Fakes the httpcontext for unit testing
    /// </summary>
    public class FakeHttpContext
    {
        private Mock<HttpContext> _httpContextMock { get; set; }

        public HttpContext Current
        {
            get
            {
                return _httpContextMock.Object;
            }
        }

        public FakeHttpContext(string url)
        {
            var contextMock = new HttpContextMock();
            contextMock.SetupUrl(url);

            var uri = new Uri(url);

            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(x => x.Request.Path).Returns(uri.AbsolutePath);

            var sessionMock = new Mock<ISession>();
            _httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

            var requestCookieMock = new Mock<IRequestCookieCollection>();
            _httpContextMock.Setup(x => x.Request.Cookies).Returns(requestCookieMock.Object);

            _httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());

            var responseMock = new Mock<HttpResponse>();
            _httpContextMock.Setup(x => x.Response).Returns(responseMock.Object);

            var responseCookieMock = new Mock<IResponseCookies>();
            _httpContextMock.Setup(x => x.Response.Cookies).Returns(responseCookieMock.Object);
        }

        public void AddCookie(string name, string value = null)
        {
            var contextMock = new HttpContextMock();
            contextMock.SetupRequestCookies(new Dictionary<string, string> {
                { name, value }
            });

            _httpContextMock.Setup(x => x.Request.Cookies).Returns(contextMock.Request.Cookies);
        }
    }
}
