using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;
using EndianTools;
using CyberBackendLibrary.DataTypes;
using System;

namespace HomeTools.Crypto
{
    public class LIBSECURE
    {
        public static byte[]? InitiateLibsecureXTEACTRBlock(byte[] BlkBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length == 16 && m_iv.Length == 8 && BlkBytes.Length <= 8)
            {
                byte[] nulledBytes = new byte[8];

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("LIBSECUREXTEA/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(EndianUtils.EndianSwap(KeyBytes)), EndianUtils.EndianSwap(m_iv))); // Bouncy Castle not like padding in decrypt mode with custom data.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(EndianUtils.EndianSwap(nulledBytes), 0, nulledBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return new ToolsImpl().Crypt_Decrypt(BlkBytes, EndianUtils.EndianSwap(ciphertextBytes), 8);
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateLibsecureXTEACTRBlock - Invalid BlkBytes, KeyByes or IV!");

            return null;
        }

        public static string MemXOR(string IV, string block, int blocksize)
        {
            StringBuilder? CryptoBytes = new();

            switch (blocksize)
            {
                case 2:
                    for (int i = 1; i != 0; --i)
                    {
                        string BlockIV = IV[..1];
                        string CipherBlock = block[..1];
                        IV = IV[1..];
                        block = block[1..];
                        try
                        {
                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                        }
                    }
                    break;
                case 4:
                    for (int i = 2; i != 0; --i)
                    {
                        string BlockIV = IV[..2];
                        string CipherBlock = block[..2];
                        IV = IV[2..];
                        block = block[2..];
                        try
                        {
                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                        }
                    }
                    break;
                case 8:
                    for (int i = 4; i != 0; --i)
                    {
                        string BlockIV = IV[..4];
                        string CipherBlock = block[..4];
                        IV = IV[4..];
                        block = block[4..];
                        try
                        {
                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                        }
                    }
                    break;
                case 16:
                    for (int i = 8; i != 0; --i)
                    {
                        string BlockIV = IV[..8];
                        string CipherBlock = block[..8];
                        IV = IV[8..];
                        block = block[8..];
                        try
                        {
                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                (Convert.ToUInt32(BlockIV, 16) ^ Convert.ToUInt32(CipherBlock, 16)).ToString("X8"))));
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
                        }
                    }
                    break;
            }

            return CryptoBytes.ToString();
        }
    }
}
