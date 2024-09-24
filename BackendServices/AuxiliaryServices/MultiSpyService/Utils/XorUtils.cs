using System.Text;

namespace MultiSpyService.Utils
{
    public class XorUtils
    {
        /// <summary>
        /// simple xor encoding for Gstats,GPSP,GPCM
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] Encrypt_Decrypt(byte[] plaintext, int type)
        {
            int index = 0;
            int length = plaintext.Length;

            byte[] KeyData = type switch
            {
                1 => Encoding.UTF8.GetBytes("GameSpy3D"),
                2 => Encoding.UTF8.GetBytes("Industries"),
                3 => Encoding.UTF8.GetBytes("ProjectAphex"),
                _ => Encoding.UTF8.GetBytes("gamespy"),
            };

            for (int i = 0; length > 0; length--)
            {
                if (i >= KeyData.Length)
                    i = 0;

                plaintext[index++] ^= KeyData[i++];
            }

            return plaintext;
        }
    }
}
