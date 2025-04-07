﻿using HTTPServer.Models;

namespace HTTPServer
{
    public enum HttpHeader
    {
        Location
    }

    public class HttpBuilder
    {
        #region Public Methods

        public static HttpResponse Ok()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Ok,
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

        public static HttpResponse NotFound()
        {
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.NotFound,
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
        #endregion
    }
}
