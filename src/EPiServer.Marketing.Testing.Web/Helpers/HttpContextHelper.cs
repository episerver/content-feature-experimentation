using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// interacts with the httpcontext for reading and manipulating the objects therein
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IHttpContextHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    [ExcludeFromCodeCoverage]
    public class HttpContextHelper : IHttpContextHelper
    {
        private readonly Injected<IHttpContextAccessor> _httpContextAccessor;
        public bool HasItem(string itemId)
        {
            return _httpContextAccessor.Service.HttpContext.Items.ContainsKey(itemId);
        }

        public string GetRequestParam(string itemId)
        {
            if (_httpContextAccessor.Service.HttpContext.Request.Query[itemId].Count > 0)
                return _httpContextAccessor.Service.HttpContext.Request.Query[itemId].ToString();

            if (_httpContextAccessor.Service.HttpContext.Request.Cookies[itemId] != null)
                return _httpContextAccessor.Service.HttpContext.Request.Cookies[itemId];
            
            return _httpContextAccessor.Service.HttpContext.GetServerVariable(itemId);
        }

        public void SetItemValue(string itemId, object value)
        {
            _httpContextAccessor.Service.HttpContext.Items[itemId] = value;
        }

        public void RemoveItem(string itemId)
        {
            _httpContextAccessor.Service.HttpContext.Items.Remove(itemId);
        }

        public bool HasCookie(string cookieKey)
        {
            foreach (var headers in _httpContextAccessor.Service.HttpContext.Response.Headers.Values)
                foreach (var header in headers)
                    if (header.StartsWith($"{cookieKey}="))
                    {
                        return true;
                    }
            return HasItem(cookieKey);
        }

        public string GetCookieValue(string cookieKey)
        {
            var value = GetResponseCookie(cookieKey);

            var pattern = "\\r|\\n|%0d|%0a";
            var substrings = Regex.Split(value, pattern, RegexOptions.None, TimeSpan.FromSeconds(2));
            
            return substrings.FirstOrDefault();
        }

        public string GetResponseCookie(string cookieName)
        {
            string cookieValue = string.Empty;
            foreach (var headers in _httpContextAccessor.Service.HttpContext.Response.Headers.Values)
                foreach (var header in headers)
                    if (header.StartsWith($"{cookieName}="))
                    {
                        var p1 = header.IndexOf('=');
                        var p2 = header.IndexOf(';');
                        cookieValue = header.Substring(p1 + 1, p2 - p1 - 1);
                        break;
                    }

            // Cookie added to Response.Cookies is not immediately available in the Request.Cookies
            // Add it to HttpContext.Items so it's available for the first visit.
            if (string.IsNullOrEmpty(cookieValue) && HasItem(cookieName))
                return _httpContextAccessor.Service.HttpContext.Items[cookieName]?.ToString();

            return cookieValue;
        }

        public string GetRequestCookie(string cookieKey)
        {
            return _httpContextAccessor.Service.HttpContext.Request.Cookies[cookieKey];
        }

        public string[] GetRequestCookieKeys()
        {
            return _httpContextAccessor.Service.HttpContext.Request.Cookies.Select(x => x.Key).ToArray();
        }

        public void RemoveCookie(string cookieKey)
        {
            _httpContextAccessor.Service.HttpContext.Response.Cookies.Delete(cookieKey);
            _httpContextAccessor.Service.HttpContext.Items.Remove(cookieKey);
        }

        public void AddCookie(string key, string value, CookieOptions options)
        {
            _httpContextAccessor.Service.HttpContext.Response.Cookies.Append(key, value, options);
        }

        public bool CanWriteToResponse()
        {
            return _httpContextAccessor.Service.HttpContext.Response.Body.CanWrite;
        }

        public Stream GetResponseFilter()
        {
            return _httpContextAccessor.Service.HttpContext.Response.Body;
        }

        public void SetResponseFilter(Stream stream)
        {
            _httpContextAccessor.Service.HttpContext.Response.Body = stream;
        }

        public bool HasCurrentContext()
        {
            return _httpContextAccessor.Service.HttpContext != null;
        }

        public bool HasUserAgent()
        {
            return _httpContextAccessor.Service.HttpContext.Request.Headers["User-Agent"].Count > 0;
        }

        public string RequestedUrl()
        {
            return _httpContextAccessor.Service.HttpContext.Request.Path;
        }

        public ContentReference GetCurrentContentLink()
        {
            ContentReference retReference = null;
            if (_httpContextAccessor.Service.HttpContext != null)
            {
                retReference = _httpContextAccessor.Service.HttpContext.GetContentLink();
            }
            return retReference;
        }

        public HttpContext GetCurrentContext()
        {
            return _httpContextAccessor.Service.HttpContext;
        }

        public string GetSessionCookieName()
        {
            // returns the default cookie name if its not specified and/or if the key is completely missing from the web.config.
            return SessionDefaults.CookieName;
        }

        public Dictionary<string, string> GetCurrentCookieCollection()
        {
            Dictionary<string,string> cookies = new Dictionary<string,string>();
            var cookieCollection = GetCurrentContext()?.Request.Cookies.Select(x => x.Key).ToArray();
            foreach (var cookieKey in cookieCollection)
            {
                cookies.Add(cookieKey, GetCurrentContext().Request.Cookies[cookieKey]);
            }

            return cookies;
        }

        public string GetContentEncoding()
        {
            return _httpContextAccessor.Service.HttpContext.Response.ContentType;
        }
    }
}
