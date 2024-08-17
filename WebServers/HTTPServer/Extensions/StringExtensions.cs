using System.IO;
using System.Text;

namespace HTTPServer.Extensions
{
    public static class StringExtensions
    {
        public static Stream ToStream(this string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }
    }
}
