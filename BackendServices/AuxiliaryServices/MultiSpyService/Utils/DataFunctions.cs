using System.Text;

namespace MultiSpyService.Utils
{
    public class DataFunctions
    {
        public static byte[] StringToBytes(string data)
        {
            return Encoding.Latin1.GetBytes(data);
        }

        public static string BytesToString(byte[] data)
        {
            return Encoding.Latin1.GetString(data);
        }
    }
}
