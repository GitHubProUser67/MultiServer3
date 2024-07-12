using CyberBackendLibrary.HTTP;
using HTTPServer.Models;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Collections.Generic;
using System.Net;

namespace HTTPServer
{
    public enum HttpHeader
    {
        Location
    }

    public class HttpBuilder
    {
        #region Public Methods

        public static HttpResponse OK()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public static HttpResponse NotImplemented()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.NotImplemented,
            };
        }

        public static HttpResponse InternalServerError()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
            };
        }

        public static HttpResponse MovedPermanently(string url)
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.MovedPermanently,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse PermanantRedirect(string url)
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.PermanentRedirect,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse Found(string url, bool KeepAlive = false)
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.Found,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse RedirectFromApacheRules(string url, int statuscode)
        {
            return new HttpResponse()
            {
                HttpStatusCode = (HttpStatusCode)statuscode,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse NoContent()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.NoContent,
            };
        }

        public static HttpResponse NotFound(HttpRequest request, string absolutepath, string Host, string ServerIP, string serverPort, bool HTMLResponse)
        {
            if (!HTMLResponse)
                return new HttpResponse()
                {
                    HttpStatusCode = HttpStatusCode.NotFound,
                };
            else
                return HttpResponse.Send(DefaultHTMLPages.GenerateNotFound(absolutepath, $"http://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                    HTTPServerConfiguration.HTTPStaticFolder, "Apache 2.2.22 (Unix) DAV/2", serverPort, HTTPServerConfiguration.NotFoundSuggestions).Result, "text/html", null, HttpStatusCode.NotFound);
        }

        public static HttpResponse NotAllowed()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        public static HttpResponse MethodNotAllowed()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            };
        }

        public static HttpResponse BadRequest()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            };
        }
        #endregion
    }
}
