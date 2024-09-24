using System.Collections.Generic;
using System.Collections.Specialized;

namespace HTTPSecureServerLite.Extensions
{
    public static class OtherExtensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string? k in col.AllKeys)
            {
                string? value = col[k];
                if (k != null && value != null)
                    dict.Add(k, value);
            }
            return dict;
        }
    }
}
