using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;

namespace CryptoSporidium.BARTools.UnBAR
{
    internal class LIBSECURE
    {
        public byte[]? InitiateLibSecureXTEACTRBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv, int blocksize)
        {
            if (KeyBytes != null && KeyBytes.Length == 16 && m_iv != null && m_iv.Length == 8 && FileBytes != null)
            {
                MiscUtils? utils = new();
                // Create the cipher
                IBufferedCipher? cipher = null;

                if (blocksize != 8)
                    cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/ENDIANCTR/ZEROBYTEPADDING");
                else
                    cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/ENDIANCTR/NOPADDING");

                cipher.Init(true, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv)); // Bouncy Castle not like padding in decrypt mode with custom data.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                if (BitConverter.IsLittleEndian) // KeyBytes endian check directly in libsecure.
                {
                    byte[]? returnbytes = new byte[blocksize];
                    Buffer.BlockCopy(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(ciphertextBytes, 4), 0, returnbytes, 0, returnbytes.Length);
                    utils = null;
                    return returnbytes;
                }
                else
                {
                    byte[]? returnbytes = new byte[blocksize];
                    Buffer.BlockCopy(ciphertextBytes, 0, returnbytes, 0, returnbytes.Length);
                    utils = null;
                    return returnbytes;
                }
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateLibSecureXTEACTRBuffer - Invalid FileBytes, KeyByes or IV!");

            return null;
        }

        public string MemXOR(string IV, string block, int blocksize)
        {
            string? returnstring = null;
            StringBuilder? CryptoBytes = new();

            if (blocksize == 8)
            {
                for (int i = 4; i != 0; --i)
                {
                    string BlockIV = IV.Substring(0, 4);
                    string CipherBlock = block.Substring(0, 4);
                    IV = IV.Substring(4);
                    block = block.Substring(4);
                    ushort uBlockIV = Convert.ToUInt16(BlockIV, 16);
                    ushort uCipherBlock = Convert.ToUInt16(CipherBlock, 16);

                    ushort Xor = (ushort)(uBlockIV ^ uCipherBlock);
                    string output = Xor.ToString("X4");
                    try
                    {
                        CryptoBytes.Append(MiscUtils.ByteArrayToHexString(MiscUtils.HexStringToByteArray(output)));
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
                    string BlockIV = IV.Substring(0, 8);
                    string CipherBlock = block.Substring(0, 8);
                    IV = IV.Substring(8);
                    block = block.Substring(8);
                    uint uBlockIV = Convert.ToUInt32(BlockIV, 16);
                    uint uCipherBlock = Convert.ToUInt32(CipherBlock, 16);

                    uint Xor = uBlockIV ^ uCipherBlock;
                    string output = Xor.ToString("X8");
                    try
                    {
                        CryptoBytes.Append(MiscUtils.ByteArrayToHexString(MiscUtils.HexStringToByteArray(output)));
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                    }
                }
            }

            returnstring = CryptoBytes.ToString();

            CryptoBytes = null;

            return returnstring;
        }
    }
}
