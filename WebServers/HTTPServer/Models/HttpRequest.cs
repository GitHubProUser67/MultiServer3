// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NetworkLibrary.HTTP;
using Newtonsoft.Json;

namespace HTTPServer.Models
{
    public class HttpRequest : IDisposable
    {
        private bool disposedValue;

        #region Properties

        public string Method { get; set; } = string.Empty;
        public string? RawUrlWithQuery { get; set; }
        public string IP { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "0";
        public string ServerIP { get; set; } = "127.0.0.1";
        public ushort ServerPort { get; set; } = 0;
        [JsonIgnore]
        public Stream? Data { get; set; }
        [JsonIgnore]
        public Route? Route { get; set; }
        public List<KeyValuePair<string, string>>? Headers { get; set; }

        #endregion

        #region Constructors
        public HttpRequest()
        {
            
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            return string.Format("{0} - {1}", Method, RawUrlWithQuery);
        }

        public string RetrieveHeaderValue(string headerUri, bool caseSensitive = true)
        {
            return Headers?.FirstOrDefault(h => caseSensitive ?
                h.Key.Equals(headerUri) :
                h.Key.Equals(headerUri, StringComparison.InvariantCultureIgnoreCase))
                .Value ?? string.Empty;
        }

        public string GetContentType()
        {
            return Headers?.FirstOrDefault(h => h.Key.Equals("content-type", StringComparison.InvariantCultureIgnoreCase))
                .Value ?? string.Empty;
        }

        public string GetContentLength()
        {
            return Headers?.FirstOrDefault(h => h.Key.Equals("content-length", StringComparison.InvariantCultureIgnoreCase))
                .Value ?? string.Empty;
        }

        public string? GetPath()
        {
            if (!string.IsNullOrEmpty(RawUrlWithQuery))
            {
                string url = HTTPProcessor.DecodeUrl(RawUrlWithQuery);

                if (Route != null && Route.UrlRegex != null)
                {
                    Match match = Regex.Match(url, Route.UrlRegex);
                    if (match.Groups.Count > 1)
                        return match.Groups[1].Value;
                }

                return url;
            }

            return null;
        }

        public Dictionary<string, string>? QueryParameters
        {
            get
            {
                if (!string.IsNullOrEmpty(RawUrlWithQuery))
                {
                    Dictionary<string, string> parameterDictionary = new();

                    int questionMarkIndex = RawUrlWithQuery.IndexOf("?");
                    if (questionMarkIndex != -1) // If '?' is found
                    {
                        string trimmedurl = RawUrlWithQuery[(questionMarkIndex + 1)..];
                        foreach (string? UrlArg in System.Web.HttpUtility.ParseQueryString(trimmedurl).AllKeys) // Thank you WebOne.
                        {
                            if (!string.IsNullOrEmpty(UrlArg))
                                parameterDictionary[UrlArg] = System.Web.HttpUtility.ParseQueryString(trimmedurl)[UrlArg] ?? string.Empty;
                        }
                    }

                    return parameterDictionary;
                }

                return null;
            }
        }

#if true // Serve as a HTTP json debugging.
        [JsonIgnore]
#endif
        public string DataAsBase64
        {
            get
            {
                if (Data != null)
                {
                    int read = 0;
                    byte[] buffer = new byte[16 * 1024];
                    using MemoryStream ms = new();
                    while ((read = Data.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }

                return string.Empty;
            }
        }

        [JsonIgnore]
        public string DataAsString
        {
            get
            {
                if (Data != null)
                {
                    int read = 0;
                    byte[] buffer = new byte[16 * 1024];
                    using MemoryStream ms = new();
                    while ((read = Data.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

                return string.Empty;
            }
        }

        [JsonIgnore]
        public byte[] DataAsBytes
        {
            get
            {
                if (Data != null)
                {
                    int read = 0;
                    byte[] buffer = new byte[16 * 1024];
                    using MemoryStream ms = new();
                    while ((read = Data.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }

                return Array.Empty<byte>();
            }
        }

        [JsonIgnore]
        public Stream? GetDataStream
        {
            get
            {
                return Data;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RawUrlWithQuery = null;
                    Route = null;
                    Headers = null;
                    try
                    {
                        Data?.Close();
                        Data?.Dispose();
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
        // ~HttpRequest()
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
