using NetworkLibrary.HTTP;
using HTTPServer.Models;
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
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse NotImplemented()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.NotImplemented,
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse InternalServerError()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                ContentAsUTF8 = string.Empty
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
                },
                ContentAsUTF8 = string.Empty
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
                },
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse Found(string url)
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.Found,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                },
                ContentAsUTF8 = string.Empty
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
                },
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse NoContent()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.NoContent,
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse NotFound(HttpRequest request, string absolutepath, string Host, bool HTMLResponse)
        {
            if (!HTMLResponse)
                return new HttpResponse()
                {
                    HttpStatusCode = HttpStatusCode.NotFound,
                    ContentAsUTF8 = string.Empty
                };
            else
                return HttpResponse.Send(DefaultHTMLPages.GenerateNotFound(absolutepath, $"http://{(string.IsNullOrEmpty(Host) ? (request.ServerIP.Length > 15 ? "[" + request.ServerIP + "]" : request.ServerIP) : Host)}",
                    HTTPServerConfiguration.HTTPStaticFolder, "Apache 2.2.22 (Unix) DAV/2", request.ServerPort.ToString(), HTTPServerConfiguration.NotFoundSuggestions).Result, "text/html", null, HttpStatusCode.NotFound);
        }

        public static HttpResponse NotAllowed()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse MethodNotAllowed()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
                ContentAsUTF8 = string.Empty
            };
        }

        public static HttpResponse BadRequest()
        {
            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                ContentAsUTF8 = string.Empty
            };
        }
        #endregion
    }
}
