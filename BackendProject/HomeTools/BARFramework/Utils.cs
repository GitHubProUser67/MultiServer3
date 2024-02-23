namespace BackendProject.HomeTools.BARFramework
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