// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using System.Text.RegularExpressions;

namespace HTTPServer.Models
{
    public class HttpRequest
    {
        #region Fields

        #endregion

        #region Properties

        public string? Method { get; set; }
        public string? Url { get; set; }
        public string IP { get; set; } = string.Empty;
        public byte[]? Data { get; set; }
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

        public string GetHeaderValue(string headeruri)
        {
            if (Headers.ContainsKey(headeruri))
                return Headers[headeruri];

            return string.Empty; // Make things simpler instead of null.
        }

        public string GetContentType()
        {
            if (Headers.ContainsKey("Content-Type"))
                return Headers["Content-Type"];
            else if (Headers.ContainsKey("Content-type"))
                return Headers["Content-type"];

            return string.Empty;
        }

        public string? GetPath()
        {
            if (Route != null && Route.UrlRegex != null && Url != null)
            {
                var match = Regex.Match(Url, Route.UrlRegex);
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
                    string queryString = Url[(Url.IndexOf("?") + 1)..];
                    string[] parameters = queryString.Split('&');

                    var parameterDictionary = new Dictionary<string, string>();

                    foreach (var parameter in parameters)
                    {
                        string[] keyValue = parameter.Split('=');

                        if (keyValue.Length == 2)
                            parameterDictionary[keyValue[0]] = keyValue[1];
                        else
                            parameterDictionary[keyValue[0]] = string.Empty;
                    }

                    return parameterDictionary;
                }

                return null;
            }
        }

        public Stream? getDataStream
        {
            get
            {
                if (Data != null)
                    return new MemoryStream(Data);
                else
                    return null;
            }
        }
        #endregion
    }
}
