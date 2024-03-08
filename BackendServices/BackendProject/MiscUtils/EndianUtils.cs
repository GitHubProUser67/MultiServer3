namespace BackendProject.MiscUtils
{
    public class EndianUtils
    {
        /// <summary>
        /// Reverse the endianess of a given byte array.
        /// <para>change l'endianess d'un tableau de bytes.</para>
        /// </summary>
        /// <param name="dataIn">The byte array to endian-swap.</param>
        /// <returns>A byte array.</returns>
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

        /// <summary>
        /// Reverse the endianess of a given uint.
        /// <para>change l'endianess d'un uint.</para>
        /// </summary>
        /// <param name="dataIn">The uint to endian-swap.</param>
        /// <returns>A uint.</returns>
        public static uint EndianSwap(uint dataIn)
        {
            return ((dataIn & 0x000000FFU) << 24) |
                   ((dataIn & 0x0000FF00U) << 8) |
                   ((dataIn & 0x00FF0000U) >> 8) |
                   ((dataIn & 0xFF000000U) >> 24);
        }

        /// <summary>
        /// Reverse the endianess of a given int.
        /// <para>change l'endianess d'un int.</para>
        /// </summary>
        /// <param name="dataIn">The int to endian-swap.</param>
        /// <returns>A uint.</returns>
        public static int EndianSwap(int dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
