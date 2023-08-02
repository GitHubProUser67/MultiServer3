namespace PSMultiServer.CryptoSporidium.BAR
{
    public static class Utils
    {
        public static long GetFourByteAligned(long input)
        {
            long num = input & ~3L; // Use bitwise AND operator to clear the lowest two bits

            if (num < input)
            {
                num = input + 4L & ~3L; // Add 4 and clear the lowest two bits
            }

            return num;
        }

        public static byte[] EndianSwap(byte[] dataIn)
        {
            int num = dataIn.Length;
            byte[] array = new byte[num];
            Array.Copy(dataIn, array, dataIn.Length);
            int num2;
            for (int i = 0; i < dataIn.Length; i += num2)
            {
                num2 = 4;
                int num3 = dataIn.Length - i;
                if (num3 < 4)
                {
                    num2 = num3;
                }
                Array.Reverse(array, i, num2);
            }
            return array;
        }
    }
}
