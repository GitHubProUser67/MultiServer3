using System.Collections.Generic;

namespace NetworkLibrary.Extension
{
    public static class ListUtils
    {
        public static List<KeyValuePair<string, string>> ConvertHeadersToPhpFriendly(this List<KeyValuePair<string, string>> headers)
        {
            List<KeyValuePair<string, string>> phpFriendlyHeaders = new List<KeyValuePair<string, string>>();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    // Convert header name to uppercase, replace dashes with underscores, and prefix with "HTTP_"
                    string phpHeaderName = "HTTP_" + header.Key.ToUpper().Replace("-", "_");

                    // Add the transformed header name and its value to the list
                    phpFriendlyHeaders.Add(new KeyValuePair<string, string>(phpHeaderName, header.Value));
                }
            }

            return phpFriendlyHeaders;
        }
    }
}
