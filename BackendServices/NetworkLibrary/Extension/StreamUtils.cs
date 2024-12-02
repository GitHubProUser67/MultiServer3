using System.IO;
using System.Text;

namespace NetworkLibrary.Extension
{
    public static class StreamUtils
    {
        public static string GetString(this Stream stream)
        {
            return Encoding.ASCII.GetString(((MemoryStream)stream).ToArray());
        }
    }
}
