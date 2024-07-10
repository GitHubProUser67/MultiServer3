using System.IO;
using System.Text;

namespace HTTPServer.Extensions
{
    public static class StringExtensions
    {
        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new();
            using (StreamWriter writer = new(stream, Encoding.UTF8, 1024, true))
            {
                writer.Write(str);
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }
    }
}
