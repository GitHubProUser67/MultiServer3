using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HomeTools.AFS
{
    public class AFSHash
    {
        public AFSHash(string text)
        {
            m_source = text;
            ComputeHash(text);
        }

        public int Value
        {
            get
            {
                return m_hash;
            }
        }

        private void ComputeHash(string text)
        {
            int hash = 0;
            foreach (char ch in text.ToLower())
            {
                char c = ch;

                if (c == '\0')
                    break;
                if (c == '\\')
                    c = '/';
                else if ((c + 0xbf) < 0x1a)
                    c = (char)(c + ' ');

                hash = hash * 37 + Convert.ToInt32(c);
            }
            m_hash = hash;
        }

        public static string EscapeString(string TextContent)
        {
            string text = Regex.Replace(TextContent, "file:(\\/+)resource_root\\/build\\/", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "file:", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "///", string.Empty, RegexOptions.IgnoreCase);
            return Regex.Replace(text, "/", "\\", RegexOptions.IgnoreCase);
        }

        private int m_hash;

        private string m_source;
    }
}