// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using System.Text.RegularExpressions;

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
        public Stream? Data { get; set; }
        public Route? Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Constructors
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            return string.Format("{0} - {1}", Method, Url);
        }

        public string RetrieveHeaderValue(string headeruri)
        {
            if (Headers.TryGetValue(headeruri, out string? value))
                return value;

            return string.Empty; // Make things simpler instead of null.
        }

        public string GetContentType()
        {
            if (Headers.TryGetValue("Content-Type", out string? value))
                return value;
            else if (Headers.TryGetValue("Content-type", out string? value1))
                return value1;

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
