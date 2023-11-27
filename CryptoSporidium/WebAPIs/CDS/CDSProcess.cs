using CryptoSporidium.BARTools.UnBAR;

namespace CryptoSporidium.WebAPIs.CDS
{
    public class CDSProcess
    {
        public static byte[]? CDSEncrypt_Decrypt(byte[] buffer, string sha1)
        {
            MiscUtils? utils = new();
            byte[]? digest = utils.ConvertSha1StringToByteArray(sha1.ToUpper());
            utils = null;
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
    }
}
