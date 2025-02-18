using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace NetworkLibrary.Extension
{
    public static class DictionaryUtils
    {
        public static string ToHttpHeaders(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string k in col.AllKeys)
            {
                string value = col[k];
                if (k != null && value != null)
                    dict.Add(k, value);
            }
            return dict;
        }
    }
}
