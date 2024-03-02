using HTTPServer.Models;

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
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public static HttpResponse NotImplemented()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.NotImplemented,
            };
        }

        public static HttpResponse InternalServerError()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
            };
        }

        public static HttpResponse MovedPermanently(string url)
        {
            return new HttpResponse(false)
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
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.PermanentRedirect,
                Headers = new Dictionary<string, string>()
                {
                    { HttpHeader.Location.ToString(), url }
                }
            };
        }

        public static HttpResponse NoContent()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.No_Content,
            };
        }

        public static HttpResponse NotFound()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Not_Found,
            };
        }

        public static HttpResponse NotAllowed()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        public static HttpResponse MethodNotAllowed()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            };
        }

        public static HttpResponse MissingParameters()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Missing_parameters,
            };
        }
        #endregion
    }
}
