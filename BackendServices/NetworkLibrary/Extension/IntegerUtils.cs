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
    }
}
