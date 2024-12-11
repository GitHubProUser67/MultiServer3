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
            byte[] KeyData;

            switch (type)
            {
                case 1:
                    KeyData = Encoding.UTF8.GetBytes("GameSpy3D");
                    break;
                case 2:
                    KeyData = Encoding.UTF8.GetBytes("Industries");
                    break;
                case 3:
                    KeyData = Encoding.UTF8.GetBytes("ProjectAphex");
                    break;
                default:
                    KeyData = Encoding.UTF8.GetBytes("gamespy");
                    break;
            }

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
