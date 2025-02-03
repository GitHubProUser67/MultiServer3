using System;

namespace NetworkLibrary.Extension
{
    public static class IntegerUtils
    {
        /// <summary>
        /// Convert array of longs back to utf-8 byte array
        /// </summary>
        /// <returns></returns>
        public static byte[] ToBytes(this uint[] l)
        {
            const byte mask = byte.MaxValue;
            byte[] b = new byte[l.Length * 4];

            // Split each long value into 4 separate characters (bytes) using the same format as ToLongs()
            for (int i = 0; i < l.Length; i++)
            {
                b[i * 4] = (byte)(l[i] & mask);
                b[(i * 4) + 1] = (byte)(l[i] >> (8 & mask));
                b[(i * 4) + 2] = (byte)(l[i] >> (16 & mask));
                b[(i * 4) + 3] = (byte)(l[i] >> (24 & mask));
            }
            return b;
        }

        public static Guid ToGuid(this int number)
        {
            byte[] bytes = new byte[16]; // 16 bytes for a GUID

            if (!BitConverter.IsLittleEndian)
                number = EndianTools.EndianUtils.ReverseInt(number);

            BitConverter.GetBytes(number).CopyTo(bytes, 12); // Store the int in the last 4 bytes

            return new Guid(bytes);
        }

        public static string ToUuid(this int number)
        {
            return $"00000000-00000000-00000000-{number:D8}";
        }
    }
}
