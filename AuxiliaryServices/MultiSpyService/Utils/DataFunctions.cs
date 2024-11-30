using System.Text;

namespace MultiSpyService.Utils
{
    public class DataFunctions
    {
        public static byte[] StringToBytes(string data)
        {
            return Encoding.GetEncoding(28591).GetBytes(data);
        }

        public static string BytesToString(byte[] data)
        {
            return Encoding.GetEncoding(28591).GetString(data);
        }
    }
}
