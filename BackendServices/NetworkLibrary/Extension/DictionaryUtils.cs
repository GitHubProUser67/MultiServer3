using System.Collections.Generic;
using System.Linq;

namespace NetworkLibrary.Extension
{
    public static class DictionaryUtils
    {
        public static string ToHttpHeaders(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }
    }
}
