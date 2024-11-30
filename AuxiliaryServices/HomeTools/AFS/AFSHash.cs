using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HomeTools.AFS
{
    public class AFSHash
    {
        public static string EscapeString(string TextContent)
        {
            string text = Regex.Replace(TextContent, "file:(\\/+)resource_root\\/build\\/", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "file:", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "///", string.Empty, RegexOptions.IgnoreCase);
            return Regex.Replace(text, "/", "\\", RegexOptions.IgnoreCase);
        }

        public static string ComputeAFSHash(string text)
        {
            int hash = 0;
            foreach (char ch in text.ToLower().Replace(Path.DirectorySeparatorChar, '/'))
                hash = hash * 37 + Convert.ToInt32(ch);
            return hash.ToString("X8");
        }
    }
}
