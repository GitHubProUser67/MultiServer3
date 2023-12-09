using CryptoSporidium.BARTools;
using CryptoSporidium.BARTools.UnBAR;
using CustomLogger;

namespace CryptoSporidium.WebAPIs.CDS
{
    public class CDSProcess
    {
        public static byte[]? CDSEncrypt_Decrypt(byte[] buffer, string sha1)
        {
            byte[]? digest = ConvertSha1StringToByteArray(sha1.ToUpper());
            if (digest != null)
            {
                ToolsImpl? toolsimpl = new();
                byte[] tranformedSHA1 = BitConverter.GetBytes(toolsimpl.Sha1toNonce(digest));
                toolsimpl = null;

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(tranformedSHA1); // Reverse the byte array for big-endian

                BlowfishCTREncryptDecrypt? blowfish = new();
                byte[]? FileBytes = blowfish.InitiateCTRBuffer(buffer, tranformedSHA1);
                blowfish = null;

                return FileBytes;
            }

            return null;
        }

        public static byte[]? CDSByteEncrypt_Decrypt(byte[] buffer, byte[] sha1)
        {
            if (sha1 != null)
            {
                ToolsImpl? toolsimpl = new();
                byte[] tranformedSHA1 = BitConverter.GetBytes(toolsimpl.Sha1toNonce(sha1));
                toolsimpl = null;

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(tranformedSHA1); // Reverse the byte array for big-endian

                BlowfishCTREncryptDecrypt? blowfish = new();
                byte[]? FileBytes = blowfish.InitiateCTRBuffer(buffer, tranformedSHA1);
                blowfish = null;

                return FileBytes;
            }

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
