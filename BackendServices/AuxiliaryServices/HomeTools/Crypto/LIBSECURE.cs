using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;
using BackendProject.MiscUtils;

namespace HomeTools.Crypto
{
    internal class LIBSECURE
    {
        public byte[]? InitiateLibsecureXTEACTRBlock(byte[] BlkBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length == 16 && m_iv.Length == 8 && BlkBytes.Length <= 8)
            {
                byte[] returnbytes = new byte[BlkBytes.Length];

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/CTR/NOPADDING");

                BlkBytes = BlkBytes.Length != 8 ? BlkBytes.Concat(new byte[8 - BlkBytes.Length]).ToArray() : BlkBytes; // Pad to 8 Zero bytes.

                cipher.Init(true, new ParametersWithIV(new KeyParameter(EndianUtils.EndianSwap(KeyBytes)), EndianUtils.EndianSwap(m_iv))); // Bouncy Castle not like padding in decrypt mode with custom data.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(BlkBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(EndianUtils.EndianSwap(BlkBytes), 0, BlkBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                Buffer.BlockCopy(EndianUtils.EndianSwap(ciphertextBytes), 0, returnbytes, 0, returnbytes.Length);
                return returnbytes;
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateLibsecureXTEACTRBuffer - Invalid FileBytes, KeyByes or IV!");

            return null;
        }

        public string MemXOR(string IV, string block, int blocksize)
        {
            StringBuilder? CryptoBytes = new();

            if (blocksize == 8)
            {
                for (int i = 4; i != 0; --i)
                {
                    string BlockIV = IV[..4];
                    string CipherBlock = block[..4];
                    IV = IV[4..];
                    block = block[4..];
                    try
                    {
                        CryptoBytes.Append(VariousUtils.ByteArrayToHexString(VariousUtils.HexStringToByteArray(
                            ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }
            else if (blocksize == 16)
            {
                for (int i = 4; i != 0; --i)
                {
                    string BlockIV = IV[..8];
                    string CipherBlock = block[..8];
                    IV = IV[8..];
                    block = block[8..];
                    try
                    {
                        CryptoBytes.Append(VariousUtils.ByteArrayToHexString(VariousUtils.HexStringToByteArray(
                            (Convert.ToUInt32(BlockIV, 16) ^ Convert.ToUInt32(CipherBlock, 16)).ToString("X8"))));
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }

            return CryptoBytes.ToString();
        }
    }
}
