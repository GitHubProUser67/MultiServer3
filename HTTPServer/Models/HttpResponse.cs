// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using HTTPServer.Extensions;
using System.Text;

namespace HTTPServer.Models
{
    public enum HttpStatusCode
    {
        // for a full list of status codes, see..
        // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

        Continue = 100,
        Ok = 200,
        Created = 201,
        Accepted = 202,
        PartialContent = 206,
        MovedPermanently = 301,
        Found = 302,
        PermanentRedirect = 308,
        NotModified = 304,
        BadRequest = 400,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        RangeNotSatisfiable = 416,
        InternalServerError = 500,
        BadGateway = 502,
        ServiceUnavailable = 503
    }

    public class HttpResponse
    {
        #region Properties

        public HttpStatusCode HttpStatusCode { get; set; }
        public Stream? ContentStream { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Constructors

        public HttpResponse(bool keepalive)
        {
            if (keepalive)
                Headers = new Dictionary<string, string>
                {
                    { "Connection", "Keep-Alive" }
                };
            else
                Headers = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format("{0} {1}", (int)HttpStatusCode, HttpStatusCode.ToString());
        }

        public static HttpResponse Send(string? stringtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.Ok)
        {
            var response = new HttpResponse(false);
            response.HttpStatusCode = statuscode;
            response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (var innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                    {
                        // Extract two values from the inner array
                        string value1 = innerArray[0];
                        string value2 = innerArray[1];
                        response.Headers.Add(value1, value2);
                    }
                }
            }
            if (stringtosend != null)
                response.ContentAsUTF8 = stringtosend;
            else
                response.ContentStream = null;

            return response;
        }

        public static HttpResponse Send(byte[]? bytearraytosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.Ok)
        {
            var response = new HttpResponse(false);
            response.HttpStatusCode = statuscode;
            response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (var innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                    {
                        // Extract two values from the inner array
                        string value1 = innerArray[0];
                        string value2 = innerArray[1];
                        response.Headers.Add(value1, value2);
                    }
                }
            }
            if (bytearraytosend != null)
                response.ContentStream = new MemoryStream(bytearraytosend);
            else
                response.ContentStream = null;

            return response;
        }

        public static HttpResponse Send(Stream? streamtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.Ok)
        {
            var response = new HttpResponse(false);
            response.HttpStatusCode = statuscode;
            response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (var innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                    {
                        // Extract two values from the inner array
                        string value1 = innerArray[0];
                        string value2 = innerArray[1];
                        response.Headers.Add(value1, value2);
                    }
                }
            }
            if (streamtosend != null)
                response.ContentStream = streamtosend;
            else
                response.ContentStream = null;

            return response;
        }

        public string ContentAsUTF8
        {
            set
            {
                ContentStream = value.ToStream();
            }
        }

        public string ToHeader()
        {
            StringBuilder strBuilder = new();

            if ((int)HttpStatusCode == 206)
                strBuilder.Append(string.Format("HTTP/{0} {1} {2}\r\n", ConfigurationDefaults.HttpVersion, 206, "Partial Content"));
            else
                strBuilder.Append(string.Format("HTTP/{0} {1} {2}\r\n", ConfigurationDefaults.HttpVersion, (int)HttpStatusCode, HttpStatusCode.ToString()));
            strBuilder.Append(Headers.ToHttpHeaders());
            strBuilder.Append("\r\n\r\n");

            return strBuilder.ToString();
        }

        public bool IsValid()
        {
            if (ContentStream == null)
                return false;

            return true;
        }

        #endregion
    }
}
