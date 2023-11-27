namespace Org.BouncyCastle.util
{
    public class EndianTools
    {
        public static byte[] ReverseEndiannessInChunks(byte[] byteArray, int chunkSize)
        {
            if (byteArray.Length < chunkSize)
                return byteArray;

            byte[] reversedArray = new byte[byteArray.Length];

            for (int i = 0; i < byteArray.Length; i += chunkSize)
            {
                int endIndex = System.Math.Min(i + chunkSize, byteArray.Length);
                for (int j = i; j < endIndex; j++)
                {
                    reversedArray[j] = byteArray[endIndex - 1 - j + i];
                }
            }

            return reversedArray;
        }
    }
}
