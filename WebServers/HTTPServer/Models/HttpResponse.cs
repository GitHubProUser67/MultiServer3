// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using NetworkLibrary.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HTTPServer.Models
{
    public class HttpResponse : IDisposable
    {
        private bool disposedValue;
        #region Properties

        public HttpStatusCode HttpStatusCode { get; set; }
        [JsonIgnore]
        public Stream? ContentStream { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        private string HttpVersion { get; set; }
        #endregion

        #region Constructors

        public HttpResponse(string? HttpVersionOverride = null, bool disableChunkedEncoding = false)
        {
            HttpVersion = (!string.IsNullOrEmpty(HttpVersionOverride)) ? HttpVersionOverride : HTTPServerConfiguration.HttpVersion;

            Headers = new Dictionary<string, string>();

            if (HTTPServerConfiguration.ChunkedTransfers && !disableChunkedEncoding && HttpVersion.Equals("1.1"))
                Headers.Add("Transfer-Encoding", "chunked");
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format("{0} {1}", (int)HttpStatusCode, HttpStatusCode.ToString());
        }

        public static HttpResponse Send(string? stringtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK, bool lowerCaseContentType = false)
        {
            HttpResponse response = new()
            {
                HttpStatusCode = statuscode
            };
            if (lowerCaseContentType)
                response.Headers["content-type"] = mimetype;
            else
                response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (string[] innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                        // Extract two values from the inner array
                        response.Headers.Add(innerArray[0], innerArray[1]);
                }
            }
            if (!string.IsNullOrEmpty(stringtosend))
                response.ContentAsUTF8 = stringtosend;
            else
                response.ContentAsUTF8 = string.Empty;

            return response;
        }

        public static HttpResponse Send(byte[]? bytearraytosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK, bool lowerCaseContentType = false)
        {
            HttpResponse response = new()
            {
                HttpStatusCode = statuscode
            };
            if (lowerCaseContentType)
                response.Headers["content-type"] = mimetype;
            else
                response.Headers["Content-Type"] = mimetype;
            if (HeaderInput != null)
            {
                foreach (var innerArray in HeaderInput)
                {
                    // Ensure the inner array has at least two elements
                    if (innerArray.Length >= 2)
                    {
                        // Extract two values from the inner array
                        string keyValue = innerArray[0];
                        if (!response.Headers.ContainsKey(keyValue))
                            response.Headers.Add(keyValue, innerArray[1]);
                    }
                }
            }
            if (bytearraytosend != null)
                response.ContentStream = new MemoryStream(bytearraytosend);
            else
                response.ContentAsUTF8 = string.Empty;

            return response;
        }

        public static HttpResponse Send(Stream? streamtosend, string mimetype = "text/plain", string[][]? HeaderInput = null, HttpStatusCode statuscode = HttpStatusCode.OK, bool lowerCaseContentType = false)
        {
            HttpResponse response = new()
            {
                HttpStatusCode = statuscode
            };
            if (lowerCaseContentType)
                response.Headers["content-type"] = mimetype;
            else
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
                {
                    response.ContentStream = new HugeMemoryStream(streamtosend, HTTPServerConfiguration.BufferSize)
                    {
                        Position = 0
                    };
                    streamtosend.Close();
                    streamtosend.Dispose();
                }
            }
            else
                response.ContentAsUTF8 = string.Empty;

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

            strBuilder.Append(string.Format("HTTP/{0} {1} {2}\r\n", HttpVersion, (int)HttpStatusCode, HttpStatusCode.ToString()));
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
