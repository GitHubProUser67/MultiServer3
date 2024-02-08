// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using BackendProject.MiscUtils;
using HTTPServer.Extensions;
using System.Text;

namespace HTTPServer.Models
{
    public enum HttpStatusCode
    {
        // for a full list of status codes, see..
        // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

        Continue = 100,
        OK = 200,
        Created = 201,
        Accepted = 202,
        Partial_Content = 206,
        MovedPermanently = 301,
        Found = 302,
        PermanentRedirect = 308,
        Not_Modified = 304,
        BadRequest = 400,
        Forbidden = 403,
        Not_Found = 404,
        MethodNotAllowed = 405,
        RangeNotSatisfiable = 416,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503
    }

    public class HttpResponse : IDisposable
    {
        private bool disposedValue;
        #region Properties

        public HttpStatusCode HttpStatusCode { get; set; }
        public Stream? ContentStream { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Constructors

        public HttpResponse(bool keepalive, string? HttpVersionOverride = null)
        {
            string HttpVersion = (!string.IsNullOrEmpty(HttpVersionOverride)) ? HttpVersionOverride : HTTPServerConfiguration.HttpVersion;

            if (keepalive)
                Headers = new Dictionary<string, string>
                {
                    { "Connection", "Keep-Alive" }
                };
            else
                Headers = new Dictionary<string, string>();

            if (HttpVersion == "1.1")
                Headers.Add("Transfer-Encoding", "chunked");
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format("{0} {1}", (int)HttpStatusCode, HttpStatusCode.ToString());
        }

        public static HttpResponse Send(string? stringtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK)
        {
            HttpResponse response = new(false)
            {
                HttpStatusCode = statuscode
            };
            response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (string[] innerArray in HeaderInput)
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

        public static HttpResponse Send(byte[]? bytearraytosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK)
        {
            HttpResponse response = new(false)
            {
                HttpStatusCode = statuscode
            };
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

        public static HttpResponse Send(Stream? streamtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK, string? HttpVersionOverride = null)
        {
            HttpResponse response = new(false, HttpVersionOverride)
            {
                HttpStatusCode = statuscode
            };
            response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (string[]? innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                        // Extract two values from the inner array
                        response.Headers.Add(innerArray[0], innerArray[1]);
                }
            }
            if (streamtosend != null)
            {
                if (streamtosend.CanSeek)
                    response.ContentStream = streamtosend;
                else
                    response.ContentStream = new HugeMemoryStream(streamtosend, HTTPServerConfiguration.BufferSize);
            }
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

            strBuilder.Append(string.Format("HTTP/{0} {1} {2}\r\n", HTTPServerConfiguration.HttpVersion, (int)HttpStatusCode, HttpStatusCode.ToString().Replace("_", " ")));
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        ContentStream?.Close();
                        ContentStream?.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Always check for disposed object according to the C# documentation.
                    }
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~HttpResponse()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
