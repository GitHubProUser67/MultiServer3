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
        public static byte[]? InitiateXTEABuffer(byte[] FileBytes, byte[] KeyBytes, byte[]? m_iv, string mode, bool memxor = true, bool encrypt = false)
        {
            if (KeyBytes.Length == 16)
            {
                byte[] nulledBytes = new byte[FileBytes.Length];

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher($"LIBSECUREXTEA/{mode}/NOPADDING");

                if (mode == "CTR" || mode == "CBC")
                {
                    if (m_iv == null || m_iv.Length != 8)
                    {
                        LoggerAccessor.LogError("[LIBSECURE] - InitiateXTEABuffer - Invalid IV!");
                        return null;
                    }

                    cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(EndianUtils.EndianSwap(KeyBytes)), EndianUtils.EndianSwap(m_iv)));
                }
                else
                    cipher.Init(encrypt, new KeyParameter(EndianUtils.EndianSwap(KeyBytes)));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(memxor ? nulledBytes : EndianUtils.EndianSwap(FileBytes), 0, nulledBytes.Length, ciphertextBytes, 0); // Little optimization for nulled bytes array, no need to endian swap a bunch of nulls.
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return memxor ? ToolsImpl.Crypt_Decrypt(FileBytes, EndianUtils.EndianSwap(ciphertextBytes), 8) : EndianUtils.EndianSwap(ciphertextBytes);
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateXTEABuffer - Invalid KeyByes!");

            return null;
        }

        public static byte[]? InitiateBlowfishBuffer(byte[] FileBytes, byte[] KeyBytes, byte[]? m_iv, string mode, bool memxor = true, bool encrypt = false)
        {
            if (KeyBytes.Length == 32)
            {
                byte[] nulledBytes = new byte[FileBytes.Length];

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher($"Blowfish/{mode}/NOPADDING");

                if (mode == "CTR" || mode == "CBC")
                {
                    if (m_iv == null || m_iv.Length != 8)
                    {
                        LoggerAccessor.LogError("[LIBSECURE] - InitiateBlowfishBuffer - Invalid IV!");
                        return null;
                    }

                    cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));
                }
                else
                    cipher.Init(encrypt, new KeyParameter(KeyBytes));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(memxor ? nulledBytes : FileBytes, 0, nulledBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return memxor ? ToolsImpl.Crypt_Decrypt(FileBytes, ciphertextBytes, 8) : ciphertextBytes;
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateBlowfishBuffer - Invalid KeyByes!");

            return null;
        }

        public static byte[]? InitiateAESBuffer(byte[] FileBytes, byte[] KeyBytes, byte[]? m_iv, string mode, bool memxor = true, bool encrypt = false)
        {
            if (KeyBytes.Length >= 16)
            {
                byte[] nulledBytes = new byte[FileBytes.Length];

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher($"AES/{mode}/NOPADDING");

                if (mode == "CTR" || mode == "CBC")
                {
                    if (m_iv == null || m_iv.Length != 16)
                    {
                        LoggerAccessor.LogError("[LIBSECURE] - InitiateAESBuffer - Invalid IV!");
                        return null;
                    }

                    cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));
                }
                else
                    cipher.Init(encrypt, new KeyParameter(KeyBytes));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(memxor ? nulledBytes : FileBytes, 0, nulledBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return memxor ? ToolsImpl.Crypt_Decrypt(FileBytes, ciphertextBytes, 16) : ciphertextBytes;
            }
            else
                LoggerAccessor.LogError("[LIBSECURE] - InitiateAESBuffer - Invalid KeyByes!");

            return null;
        }

        public static string MemXOR(string IV, string block, int blocksize)
        {
            StringBuilder? CryptoBytes = new();

            try
            {
                switch (blocksize)
                {
                    case 2:
                        for (int i = 1; i != 0; --i)
                        {
                            string BlockIV = IV[..4];
                            string CipherBlock = block[..4];
                            IV = IV[4..];
                            block = block[4..];

                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                    ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        break;
                    case 4:
                        for (int i = 2; i != 0; --i)
                        {
                            string BlockIV = IV[..4];
                            string CipherBlock = block[..4];
                            IV = IV[4..];
                            block = block[4..];

                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                    ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        break;
                    case 8:
                        for (int i = 4; i != 0; --i)
                        {
                            string BlockIV = IV[..4];
                            string CipherBlock = block[..4];
                            IV = IV[4..];
                            block = block[4..];

                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                    ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        break;
                    case 16:
                        for (int i = 8; i != 0; --i)
                        {
                            string BlockIV = IV[..4];
                            string CipherBlock = block[..4];
                            IV = IV[4..];
                            block = block[4..];

                            CryptoBytes.Append(DataTypesUtils.ByteArrayToHexString(DataTypesUtils.HexStringToByteArray(
                                    ((ushort)(Convert.ToUInt16(BlockIV, 16) ^ Convert.ToUInt16(CipherBlock, 16))).ToString("X4"))));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[LIBSECURE] - Error In MemXOR: {ex}");
            }

            return CryptoBytes.ToString();
        }
    }
}
