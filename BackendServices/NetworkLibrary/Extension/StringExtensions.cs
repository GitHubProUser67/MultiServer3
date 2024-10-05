using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkLibrary.Extension
{
    public static class StringExtensions
    {
        public static string ChopOffBefore(this string s, string Before)
        {
            // Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(Before.ToUpper());
            if (End > -1)
                return s.Substring(End + Before.Length);

            return s;
        }

        public static string ChopOffAfter(this string s, string After)
        {
            // Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(After.ToUpper());
            if (End > -1)
                return s.Substring(0, End);
            return s;
        }

        public static string ReplaceIgnoreCase(this string Source, string Pattern, string Replacement)
        {
            // using \\$ in the pattern will screw this regex up
            // return Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

            if (Regex.IsMatch(Source, Pattern, RegexOptions.IgnoreCase))
                Source = Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

            return Source;
        }

        public static Stream ToStream(this string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }
    }
}
