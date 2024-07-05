using System.IO;

namespace MozaicHTTP.Extensions
{
    public static class StringExtensions
    {
        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
