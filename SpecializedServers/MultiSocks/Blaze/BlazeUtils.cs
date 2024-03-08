namespace SRVEmu.Blaze
{
    public class BlazeUtils
    {
        public static long ConvertHex(string hex)
        {
            hex = hex.Trim();
            if (hex.StartsWith("0x"))
                hex = hex[2..];
            return Convert.ToInt64(hex, 16);
        }
    }
}
