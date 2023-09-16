using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;
using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;
using System.Text;
using System.Security.Cryptography;

namespace MultiServer.CryptoSporidium
{
    public class AFSBLOWFISH
    {
        public static byte[] INFIVA = null;

        public static void InitiateINFCryptoContext()
        {
            if (INFIVA == null)
            {
                byte[] nulledBytes = new byte[524]; // Encrypt 524 in bytes as shown in eboot.

                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(AFSMISC.MetaDataV1Key), AFSMISC.MetaDataV1IV)); // Doesn't matter in that case, since CTR is a bi-directional crypto.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(nulledBytes, 0, nulledBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                INFIVA = ciphertextBytes;
            }
        }

        public static byte[] InitiateLUACryptoContext(byte[] Headerdata, byte[] SignatureIV)
        {
            if (SignatureIV != null && SignatureIV.Length == 8 && Headerdata.Length == 24)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(AFSMISC.SignatureKey), SignatureIV)); // Doesn't matter in that case, since CTR is a bi-directional crypto.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(Headerdata.Length)];
                int ciphertextLength = cipher.ProcessBytes(Headerdata, 0, Headerdata.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                return ciphertextBytes;
            }
            else
                ServerConfiguration.LogError("[AFS] - InitiateLUACryptoContext , parameter invalid!");

            return null;
        }

        public static byte[] Encrypt_DecryptCDSContent(byte[] FileBytes, byte[] SHA1IV)
        {
            if (FileBytes != null && SHA1IV != null && SHA1IV.Length == 8)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                // We set this to true because for some ... reasons, the decrypt mode doesn't want to apply padding.
                cipher.Init(true, new ParametersWithIV(new KeyParameter(AFSMISC.DefaultKey), SHA1IV)); // Doesn't matter in that case, since CTR is a bi-directional crypto.

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                return ciphertextBytes;
            }
            else
                ServerConfiguration.LogError("[AFS] - Encrypt_DecryptCDSContent , parameter invalid!");

            return null;
        }

        public static byte[] Crypt_Decrypt(byte[] fileBytes, byte[] IVA)
        {
            if (fileBytes.Length >= 4 && fileBytes[0] == 0xBE && fileBytes[1] == 0xE5 && fileBytes[2] == 0xBE && fileBytes[3] == 0xE5) // INF Magic header.
                return fileBytes;

            StringBuilder hexStr = new StringBuilder();

            if (IVA != null && IVA.Length >= 8)
            {
                int i = 8; // Start index for processing.

                while (i <= IVA.Length)
                {
                    byte[] ivBlk = new byte[8];
                    Array.Copy(IVA, i - 8, ivBlk, 0, ivBlk.Length);

                    byte[] block = new byte[8];
                    int blockLength = Math.Min(8, fileBytes.Length - (i - 8)); // Determine the block length, considering remaining bytes.
                    Array.Copy(fileBytes, i - 8, block, 0, blockLength);

                    // If the block length is less than 8, pad with ISO97971 bytes.
                    if (blockLength < 8)
                    {
                        int BytesToFill = 8 - blockLength;

                        byte[] ISO97971 = new byte[BytesToFill];

                        for (int j = 0; j < BytesToFill; j++)
                        {
                            if (j == 0)
                                ISO97971[j] = 0x80;
                            else if ( j == BytesToFill -1)
                                ISO97971[j] = 0x01;
                            else
                                ISO97971[j] = 0x00;
                        }

                        Array.Copy(ISO97971, 0, block, blockLength, ISO97971.Length); // Copy the ISO97971 padding at the beginning

                        string hexresult = LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 8);
                        hexStr.Append(hexresult.Substring(0, hexresult.Length - (BytesToFill * 2))); // Pemdas rule necessary, and we double size because we work with bytes in a string.
                    }
                    else
                        hexStr.Append(LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 8));

                    i += 8;
                }

                return Misc.HexStringToByteArray(hexStr.ToString());
            }
            else
                ServerConfiguration.LogError("[Encrypt_Decrypt] - No IV entered or invalid length!");

            return null;
        }
    }

    public class AFSXTEA
    {
        // Todo - put profanity filter key

        public static byte[] Crypt_Decrypt(byte[] fileBytes, byte[] IVA)
        {
            StringBuilder hexStr = new StringBuilder();

            if (IVA != null && IVA.Length >= 8)
            {
                int i = 8; // Start index for processing.

                while (i <= IVA.Length)
                {
                    byte[] ivBlk = new byte[8];
                    Array.Copy(IVA, i - 8, ivBlk, 0, ivBlk.Length);

                    byte[] block = new byte[8];
                    int blockLength = Math.Min(8, fileBytes.Length - (i - 8)); // Determine the block length, considering remaining bytes.
                    Array.Copy(fileBytes, i - 8, block, 0, blockLength);

                    // If the block length is less than 8, pad with ISO97971 bytes.
                    if (blockLength < 8)
                    {
                        int BytesToFill = 8 - blockLength;

                        byte[] ISO97971 = new byte[BytesToFill];

                        for (int j = 0; j < BytesToFill; j++)
                        {
                            if (j == 0)
                                ISO97971[j] = 0x80;
                            else if (j == BytesToFill - 1)
                                ISO97971[j] = 0x01;
                            else
                                ISO97971[j] = 0x00;
                        }

                        Array.Copy(ISO97971, 0, block, blockLength, ISO97971.Length); // Copy the ISO97971 padding at the beginning

                        string hexresult = LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 8);
                        hexStr.Append(hexresult.Substring(0, hexresult.Length - (BytesToFill * 2))); // Pemdas rule necessary, and we double size because we work with bytes in a string.
                    }
                    else
                        hexStr.Append(LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 8));

                    i += 8;
                }

                return Misc.HexStringToByteArray(hexStr.ToString());
            }
            else
                ServerConfiguration.LogError("[Encrypt_Decrypt] - No IV entered or invalid length!");

            return null;
        }
    }

    public class AFSAES
    {
        public static byte[] Crypt_Decrypt(byte[] fileBytes, byte[] IVA)
        {
            StringBuilder hexStr = new StringBuilder();

            if (IVA != null && IVA.Length >= 16)
            {
                int i = 16; // Start index for processing.

                while (i <= IVA.Length)
                {
                    byte[] ivBlk = new byte[16];
                    Array.Copy(IVA, i - 16, ivBlk, 0, ivBlk.Length);

                    byte[] block = new byte[16];
                    int blockLength = Math.Min(16, fileBytes.Length - (i - 16)); // Determine the block length, considering remaining bytes.
                    Array.Copy(fileBytes, i - 16, block, 0, blockLength);

                    // If the block length is less than 8, pad with ISO97971 bytes.
                    if (blockLength < 16)
                    {
                        int BytesToFill = 16 - blockLength;

                        byte[] ISO97971 = new byte[BytesToFill];

                        for (int j = 0; j < BytesToFill; j++)
                        {
                            if (j == 0)
                                ISO97971[j] = 0x80;
                            else if (j == BytesToFill - 1)
                                ISO97971[j] = 0x01;
                            else
                                ISO97971[j] = 0x00;
                        }

                        Array.Copy(ISO97971, 0, block, blockLength, ISO97971.Length); // Copy the ISO97971 padding at the beginning

                        string hexresult = LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 16);
                        hexStr.Append(hexresult.Substring(0, hexresult.Length - (BytesToFill * 2))); // Pemdas rule necessary, and we double size because we work with bytes in a string.
                    }
                    else
                        hexStr.Append(LIBSECURE.MemXOR(Misc.ByteArrayToHexString(ivBlk), Misc.ByteArrayToHexString(block), 16));

                    i += 16;
                }

                return Misc.HexStringToByteArray(hexStr.ToString());
            }
            else
                ServerConfiguration.LogError("[Encrypt_Decrypt] - No IV entered or invalid length!");

            return null;
        }
    }

    public class AFSMISC
    {
        public static byte[] DefaultKey = new byte[]
        {
            0x80, 0x6d, 0x79, 0x16, 0x23, 0x42, 0xa1, 0x0e,
            0x8f, 0x78, 0x14, 0xd4, 0xf9, 0x94, 0xa2, 0xd1,
            0x74, 0x13, 0xfc, 0xa8, 0xf6, 0xe0, 0xb8, 0xa4,
            0xed, 0xb9, 0xdc, 0x32, 0x7f, 0x8b, 0xa7, 0x11
        };
        public static byte[] SignatureKey = new byte[]
        {
            0xef, 0x8c, 0x7d, 0xe8, 0xe5, 0xd5, 0xd6, 0x1d,
            0x6a, 0xaa, 0x5a, 0xca, 0xf7, 0xc1, 0x6f, 0xc4,
            0x5a, 0xfc, 0x59, 0xe4, 0x8f, 0xe6, 0xc5, 0x93,
            0x7e, 0xbd, 0xff, 0xc1, 0xe3, 0x99, 0x9e, 0x62
        };
        public static byte[] FakeSignatureKey = new byte[] // Insert Star Wars IT'S A TRAP here.
        {
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F
        };
        public static byte[] contentkey = new byte[]
        {
            47, 92, 237, 166, 58, 154, 103, 44,
            3, 76, 18, 225, 228, 37, 250, 129,
            22, 22, 174, 28, 230, 109, 235, 149,
            183, 230, 191, 33, 64, 71, 2, 220 
        };
        public static byte[] EncryptionKey = new byte[]
        {
            0xCF, 0x5B, 0xC6, 0x44, 0x0B, 0x9F, 0x0E, 0xB0,
            0x52, 0xB1, 0x0A, 0xC0, 0x2B, 0x0C, 0xD5, 0x07,
            0x02, 0xCB, 0x44, 0x9F, 0x4F, 0x2A, 0xBA, 0x6A,
            0x4B, 0xA4, 0xFF, 0x02, 0x5C, 0x5F, 0xAC, 0x15
        };
        public static byte[] MetaDataV1Key = new byte[]
        {
            0x8B, 0x41, 0xA7, 0xDE, 0x47, 0xA0, 0xD4, 0x45,
            0xE2, 0xA5, 0x90, 0x34, 0x3C, 0xD9, 0xA8, 0xB5,
            0x69, 0x5E, 0xFA, 0xD9, 0x97, 0x32, 0xEC, 0x56,
            0xB, 0x31, 0xE8, 0x5A, 0xD1, 0x85, 0x7C, 0x89
        };
        public static byte[] BlowfishKey = new byte[]
        {
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB
        };

        public static byte[] MetaDataV1IV = new byte[] { 0x2a, 0xa7, 0xcb, 0x49, 0x9f, 0xa1, 0xbd, 0x81 };

        public static byte[] TicketListV1IV = new byte[] { 0xc7, 0x96, 0x79, 0xe5, 0x79, 0x99, 0x9f, 0xbf };

        public static string ValidateSha1(byte[] data)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(data);
                StringBuilder sb = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Convert each byte to a hexadecimal string
                }

                return sb.ToString().ToUpper();
            }
        }

        public static byte[] ConvertSha1StringToByteArray(string sha1String)
        {
            if (sha1String.Length % 2 != 0)
                ServerConfiguration.LogError("Input string length must be even.");

            byte[] byteArray = new byte[sha1String.Length / 2];

            for (int i = 0; i < sha1String.Length; i += 2)
            {
                string hexByte = sha1String.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return byteArray;
        }

        public static ulong Sha1toNonce(byte[] digest)
        {
            ulong v1 = 0UL;
            if (digest != null && digest.Length >= 8)
                v1 = BitConverter.ToUInt64(digest, 0);
            return v1;
        }

        public static ulong BuildSignatureIv(int fileSize, int compressedSize, int dataStart, int userData)
        {
            return ((ulong)fileSize << 0x30) | (((ulong)compressedSize << 0x20) & 0xFFFF00000000UL) | (((ulong)dataStart << 0xE) & 0xFFFF0000UL) | (ushort)userData;
        }

        public static byte[] ApplyPaddingPrefix(byte[] filebytes) // Before you say anything, this is an actual Home Feature...
        {
            return Misc.Combinebytearay(new byte[] { 0x00, 0x00, 0x00, 0x01 }, filebytes);
        }

        public static byte[] RemovePaddingPrefix(byte[] fileBytes) // For Encryption Proxy, XTEA Proxy and INF files.
        {
            byte[] destinationArray = new byte[fileBytes.Length - 4]; // New array size after removing 4 elements

            // Copy the portion of the source array starting from index 4 to the destination array
            Array.Copy(fileBytes, 4, destinationArray, 0, destinationArray.Length);

            return destinationArray;
        }
    }
}
