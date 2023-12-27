namespace BackendProject.BARTools.BAR
{
    public static class Utils
    {
        public static long GetFourByteAligned(long input)
        {
            long num = input & ~3L; // Use bitwise AND operator to clear the lowest two bits

            if (num < input)
                num = input + 4L & ~3L; // Add 4 and clear the lowest two bits

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
                    num2 = num3;
                Array.Reverse(array, i, num2);
            }
            return array;
        }

        public static uint EndianSwap(uint originalValue)
        {
            return ((originalValue & 0x000000FFU) << 24) |
                   ((originalValue & 0x0000FF00U) << 8) |
                   ((originalValue & 0x00FF0000U) >> 8) |
                   ((originalValue & 0xFF000000U) >> 24);
        }

        public static int EndianSwap(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static byte[] IntToByteArray(int value)
        {
            byte[] byteArray = BitConverter.GetBytes(value); // Endian not checked.

            if (byteArray.Length < 4)
            {
                // If the byte array is less than 4 bytes, pad it with zeroes
                byte[] paddedByteArray = new byte[4];
                byteArray.CopyTo(paddedByteArray, 0);
                return paddedByteArray;
            }
            else if (byteArray.Length > 4)
            {
                // If the byte array is greater than 4 bytes, truncate it
                byte[] truncatedByteArray = new byte[4];
                Array.Copy(byteArray, truncatedByteArray, 4);
                return truncatedByteArray;
            }

            return byteArray; // The byte array is already 4 bytes long
        }
    }
}