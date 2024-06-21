using CyberBackendLibrary.HTTP;
using HTTPServer.Models;
using System.Collections.Generic;

namespace HTTPServer
{
    public enum HttpHeader
    {
        Location
    }

    public class HttpBuilder
    {
        #region Public Methods

        public static HttpResponse OK(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public static HttpResponse NotImplemented(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.NotImplemented,
            };
        }

        public static HttpResponse InternalServerError(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
            };
        }

        public static HttpResponse MovedPermanently(bool KeepAlive, string url)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.MovedPermanently,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse PermanantRedirect(bool KeepAlive, string url)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.Permanent_Redirect,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse Found(bool KeepAlive, string url)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.Found,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse RedirectFromApacheRules(bool KeepAlive, string url, int statuscode)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = (HttpStatusCode)statuscode,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse NoContent(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.No_Content,
            };
        }

        public static HttpResponse NotFound(bool KeepAlive, HttpRequest request, string absolutepath, string Host, string ServerIP, string serverPort, bool HTMLResponse)
        {
            if (!HTMLResponse)
                return new HttpResponse(request.RetrieveHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.Not_Found,
                };
            else
                return HttpResponse.Send(KeepAlive, DefaultHTMLPages.GenerateNotFound(absolutepath, $"http://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                    HTTPServerConfiguration.HTTPStaticFolder, "Apache 2.2.22 (Unix) DAV/2", serverPort, HTTPServerConfiguration.NotFoundSuggestions).Result, "text/html", null, HttpStatusCode.Not_Found);
        }

        public static HttpResponse NotAllowed(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        public static HttpResponse MethodNotAllowed(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            };
        }

        public static HttpResponse MissingParameters(bool KeepAlive)
        {
            return new HttpResponse(KeepAlive)
            {
                HttpStatusCode = HttpStatusCode.Missing_parameters,
            };
        }
        #endregion
    }
}
