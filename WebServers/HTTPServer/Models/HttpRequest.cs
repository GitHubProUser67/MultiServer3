// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace HTTPServer.Models
{
    public class HttpRequest : IDisposable
    {
        private bool disposedValue;

        #region Properties

        public string Method { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string IP { get; set; } = string.Empty;
        public string? Port { get; set; } = string.Empty;
        public ushort ServerPort { get; set; }
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
            return string.Format("{0} - {1}", Method, Url);
        }

        public string RetrieveHeaderValue(string headeruri)
        {
            // Check if Headers is null or empty first to avoid unnecessary LINQ operations
            if (Headers == null || !Headers.Any())
                return string.Empty;

            // Try to find the header with the specified key
            KeyValuePair<string, string>? header = Headers
                .FirstOrDefault(h => h.Key.Equals(headeruri));

            // Check if the header was found and its value is not the default empty string
            if (header.HasValue && !string.IsNullOrEmpty(header.Value.Value))
                return header.Value.Value;

            return string.Empty;
        }

        public string GetContentType()
        {
            // Check if Headers is null or empty first to avoid unnecessary LINQ operations
            if (Headers == null || !Headers.Any())
                return string.Empty;

            // Try to find the header with the specified key
            KeyValuePair<string, string>? header = Headers
                .FirstOrDefault(h => h.Key.Equals("content-type", StringComparison.InvariantCultureIgnoreCase));

            // Check if the header was found and its value is not the default empty string
            if (header.HasValue && !string.IsNullOrEmpty(header.Value.Value))
                return header.Value.Value;

            return string.Empty;
        }

        public string GetContentLength()
        {
            // Check if Headers is null or empty first to avoid unnecessary LINQ operations
            if (Headers == null || !Headers.Any())
                return string.Empty;

            // Try to find the header with the specified key
            KeyValuePair<string, string>? header = Headers
                .FirstOrDefault(h => h.Key.Equals("content-length", StringComparison.InvariantCultureIgnoreCase));

            // Check if the header was found and its value is not the default empty string
            if (header.HasValue && !string.IsNullOrEmpty(header.Value.Value))
                return header.Value.Value;

            return string.Empty;
        }

        public string? GetPath()
        {
            if (Route != null && Route.UrlRegex != null && Url != null)
            {
                Match match = Regex.Match(Url, Route.UrlRegex);
                if (match.Groups.Count > 1)
                    return match.Groups[1].Value;
            }

            return Url;
        }

        public Dictionary<string, string>? QueryParameters
        {
            get
            {
                if (Url != null)
                {
                    Dictionary<string, string> parameterDictionary = new();

                    int questionMarkIndex = Url.IndexOf("?");
                    if (questionMarkIndex != -1) // If '?' is found
                    {
                        string trimmedurl = Url[(questionMarkIndex + 1)..];
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
                    Url = null;
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
