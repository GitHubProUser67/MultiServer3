using HomeTools.Crypto;
using CustomLogger;
using EndianTools;
using System;

namespace HomeTools.CDS
{
    public class CDSProcess
    {
        public static byte[]? CDSEncrypt_Decrypt(byte[] buffer, string sha1)
        {
            byte[]? digest = ConvertSha1StringToByteArray(sha1.ToUpper());
            if (digest != null)
                return LIBSECURE.InitiateBlowfishBuffer(buffer, ToolsImpl.DefaultKey,
                    BitConverter.GetBytes(!BitConverter.IsLittleEndian ? EndianUtils.EndianSwap(ToolsImpl.Sha1toNonce(digest)) : ToolsImpl.Sha1toNonce(digest)), "CTR"); // Always big endian.

            return null;
        }

        private static byte[]? ConvertSha1StringToByteArray(string sha1String)
        {
            if (sha1String.Length % 2 != 0)
            {
                LoggerAccessor.LogError("Input string length must be even.");
                return null;
            }

            byte[] byteArray = new byte[sha1String.Length / 2];

            for (int i = 0; i < sha1String.Length; i += 2)
            {
                string hexByte = sha1String.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return byteArray;
        }
    }
}
