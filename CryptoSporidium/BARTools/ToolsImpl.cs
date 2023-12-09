using CustomLogger;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ComponentAce.Compression.Libs.zlib;
using CryptoSporidium.BARTools.BAR;
using CryptoSporidium.BARTools.UnBAR;

namespace CryptoSporidium.BARTools
{
    public class ToolsImpl
    {
        public static int ENCRYPT_MODE = 1;
        public static int DECRYPT_MODE = 2;

        public static byte[]? INFIVA = null;

        public static string base64DefaultSharcKey = "L1ztpjqaZywDTBLh5CX6gRYWrhzmbeuVt+a/IUBHAtw=";

        public static string base64CDNKey1 = "8243a3b10f1f1660a7fc934aac263c9c5161092dc25=";

        public static string base64CDNKey2 = "8b9qT7u6XQ7Sf0GKSIivMEeG7NROLTZGgNtN8iI6n1Y=";

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
        public static byte[] TicketListV0Key = new byte[]
        {
            4, 0x6B, 0xD9, 0x2A, 0x10, 0xAC, 0x6A, 0x3F,
            0xEB, 0x5C, 0xB7, 0x6D, 0x2C, 6, 0xCB, 0xA6,
            0xF, 0x4E, 0x64, 0x36, 0x63, 0x7B, 0xFA, 0x51,
            0x97, 0xE6, 0x2A, 0xF0, 0x76, 0xDA, 0x9E, 0xA2
        };
        public static byte[] TicketListV1Key = new byte[]
        {
            0x5B, 0xD6, 0xC7, 0xC8, 0xD6, 0x76, 0x37, 0x9E,
            0x8F, 0x54, 0xC1, 0x56, 0xE3, 0x88, 0x87, 0x8D,
            0x98, 0x6C, 0xC, 0x3E, 0x4D, 0x5D, 0x1C, 0x4B,
            0x32, 2, 0xB1, 0xE5, 0x2F, 0x81, 0x35, 0x44
        };
        public static byte[] HDKContentCreationSignatureKey = new byte[] // Insert "IT'S A TRAP" here, just made to make HDK Content Creation builds
        {                                                                // not decrypt the EncryptionProxy Signature Header.
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F
        };
        public static byte[] BlowfishKey = new byte[] // Seems to be the equivalent of "DO NOT SEND THE SIGNAL" for the default key
        {                                             // on HDK Content Creation builds.
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB
        };

        public static byte[] MetaDataV1IV = new byte[] { 0x2a, 0xa7, 0xcb, 0x49, 0x9f, 0xa1, 0xbd, 0x81 };

        public static byte[] TicketListV0IV = new byte[] { 0x30, 0x4B, 0x10, 0x3D, 0x46, 0x77, 0xAD, 0x84 };

        public static byte[] TicketListV1IV = new byte[] { 0xc7, 0x96, 0x79, 0xe5, 0x79, 0x99, 0x9f, 0xbf };

        public static void fail(string a)
        {
            LoggerAccessor.LogError($"[UnBAR] ToolsImpl Failed with error {a}");
        }

        public string ValidateSha1(byte[] data)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(data);
                StringBuilder sb = new();

                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Convert each byte to a hexadecimal string
                }

                return sb.ToString().ToUpper();
            }
        }

        public byte[] ValidateBytesSha1(byte[] data)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(data);
            }
        }

        public ulong BuildSignatureIv(int fileSize, int compressedSize, int dataStart, int userData)
        {
            return (ulong)fileSize << 0x30 | (ulong)compressedSize << 0x20 & 0xFFFF00000000UL | (ulong)dataStart << 0xE & 0xFFFF0000UL | (ushort)userData;
        }

        private static void calculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            aesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            BigInteger bigInteger1 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(numArray2));
            BigInteger bigInteger2 = (numArray2[0] & 128) == 0 ? bigInteger1 << 1 : bigInteger1 << 1 ^ new BigInteger(135);
            byte[] src1 = ConversionUtils.reverseByteWithSizeFIX(bigInteger2.ToByteArray());
            if (src1.Length >= 16)
                ConversionUtils.arraycopy(src1, src1.Length - 16, K1, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K1, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src1, 0, K1, 16 - src1.Length, src1.Length);
            }
            bigInteger2 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(K1));
            byte[] src2 = ConversionUtils.reverseByteWithSizeFIX(((K1[0] & 128) == 0 ? bigInteger2 << 1 : bigInteger2 << 1 ^ new BigInteger(135)).ToByteArray());
            if (src2.Length >= 16)
                ConversionUtils.arraycopy(src2, src2.Length - 16, K2, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K2, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src2, 0, K2, 16 - src2.Length, src2.Length);
            }
        }

        public void IncrementIVBytes(byte[] byteArray, int increment)
        {
            for (int i = byteArray.Length - 1; i >= 0; i--)
            {
                int newValue = byteArray[i] + (byte)increment;
                byteArray[i] = (byte)newValue;
                increment = newValue >> 8; // Carry over the overflow to the next byte
                if (increment == 0)
                    break; // No more overflow, we're done
            }
        }

        public byte[] ICSharpEdgeCompress(byte[] inData)
        {
            MemoryStream memoryStream = new(inData.Length);
            MemoryStream memoryStream2 = new(inData);
            while (memoryStream2.Position < memoryStream2.Length)
            {
                int num = Math.Min((int)(memoryStream2.Length - memoryStream2.Position), 65535);
                byte[] array = new byte[num];
                memoryStream2.Read(array, 0, num);
                byte[] array2 = ICSharpEdgeCompressChunk(array);
                memoryStream.Write(array2, 0, array2.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private byte[] ICSharpEdgeCompressChunk(byte[] InData)
        {
            MemoryStream memoryStream = new();
            Deflater deflater = new(9, true);
            DeflaterOutputStream deflaterOutputStream = new(memoryStream, deflater);
            deflaterOutputStream.Write(InData, 0, InData.Length);
            deflaterOutputStream.Close();
            memoryStream.Close();
            byte[] array = memoryStream.ToArray();
            byte[] array2;
            if (array.Length >= InData.Length)
                array2 = InData;
            else
                array2 = array;
            byte[] array3 = new byte[array2.Length + 4];
            Array.Copy(array2, 0, array3, 4, array2.Length);
            ChunkHeader chunkHeader = default;
            chunkHeader.SourceSize = (ushort)InData.Length;
            chunkHeader.CompressedSize = (ushort)array2.Length;
            byte[] array4 = chunkHeader.GetBytes();
            array4 = Utils.EndianSwap(array4);
            Array.Copy(array4, 0, array3, 0, ChunkHeader.SizeOf);
            return array3;
        }

        public byte[]? ICSharpEdgeZlibDecompress(byte[] inData)
        {
            MemoryStream memoryStream = new();
            MemoryStream memoryStream2 = new(inData);
            byte[] array = new byte[ChunkHeader.SizeOf];
            while (memoryStream2.Position < memoryStream2.Length)
            {
                memoryStream2.Read(array, 0, ChunkHeader.SizeOf);
                array = Utils.EndianSwap(array);
                ChunkHeader header = ChunkHeader.FromBytes(array);
                int compressedSize = header.CompressedSize;
                byte[] array2 = new byte[compressedSize];
                memoryStream2.Read(array2, 0, compressedSize);
                byte[] array3 = ICSharpDecompressEdgeZlibChunk(array2, header);
                memoryStream.Write(array3, 0, array3.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private byte[] ICSharpDecompressEdgeZlibChunk(byte[] inData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return inData;
            MemoryStream baseInputStream = new(inData);
            Inflater inf = new(true);
            InflaterInputStream inflaterInputStream = new(baseInputStream, inf);
            MemoryStream memoryStream = new();
            byte[] array = new byte[4096];
            for (; ; )
            {
                int num = inflaterInputStream.Read(array, 0, array.Length);
                if (num <= 0)
                    break;
                memoryStream.Write(array, 0, num);
            }
            inflaterInputStream.Close();
            return memoryStream.ToArray();
        }

        public byte[]? ComponentAceEdgeZlibDecompress(byte[] inData)
        {
            MemoryStream memoryStream = new();
            MemoryStream memoryStream2 = new(inData);
            byte[] array = new byte[ChunkHeader.SizeOf];
            while (memoryStream2.Position < memoryStream2.Length)
            {
                memoryStream2.Read(array, 0, ChunkHeader.SizeOf);
                array = Utils.EndianSwap(array);
                ChunkHeader header = ChunkHeader.FromBytes(array);
                int compressedSize = header.CompressedSize;
                byte[] array2 = new byte[compressedSize];
                memoryStream2.Read(array2, 0, compressedSize);
                byte[] array3 = ComponentAceDecompressEdgeZlibChunk(array2, header);
                memoryStream.Write(array3, 0, array3.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private byte[] ComponentAceDecompressEdgeZlibChunk(byte[] inData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return inData;
            MemoryStream memoryStream = new();
            ZOutputStream zoutputStream = new(memoryStream, true);
            byte[] array = new byte[inData.Length];
            Array.Copy(inData, 0, array, 0, inData.Length);
            zoutputStream.Write(array, 0, array.Length);
            zoutputStream.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        public byte[]? ComponentAceEdgeZlibCompress(byte[] inData)
        {
            MemoryStream memoryStream = new(inData.Length);
            MemoryStream memoryStream2 = new(inData);
            while (memoryStream2.Position < memoryStream2.Length)
            {
                int num = Math.Min((int)(memoryStream2.Length - memoryStream2.Position), 65535);
                byte[] array = new byte[num];
                memoryStream2.Read(array, 0, num);
                byte[] array2 = ComponentAceCompressEdgeZlibChunk(array);
                memoryStream.Write(array2, 0, array2.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private byte[] ComponentAceCompressEdgeZlibChunk(byte[] InData)
        {
            MemoryStream memoryStream = new();
            ZOutputStream zoutputStream = new(memoryStream, 9, true);
            zoutputStream.Write(InData, 0, InData.Length);
            zoutputStream.Close();
            memoryStream.Close();
            byte[] array = memoryStream.ToArray();
            byte[] array2;
            if (array.Length >= InData.Length)
                array2 = InData;
            else
                array2 = array;
            byte[] array3 = new byte[array2.Length + 4];
            Array.Copy(array2, 0, array3, 4, array2.Length);
            ChunkHeader chunkHeader = default;
            chunkHeader.SourceSize = (ushort)InData.Length;
            chunkHeader.CompressedSize = (ushort)array2.Length;
            byte[] array4 = chunkHeader.GetBytes();
            array4 = Utils.EndianSwap(array4);
            Array.Copy(array4, 0, array3, 0, ChunkHeader.SizeOf);
            return array3;
        }

        public async Task<byte[]> ProcessXTEABlocksAsync(byte[] inputArray, byte[] Key, byte[] IV)
        {
            int inputLength = inputArray.Length;
            int inputIndex = 0;
            int outputIndex = 0;
            byte[]? output = new byte[inputLength];
            SemaphoreSlim semaphore = new(1);
            ToolsImpl? toolsimpl = new();
            LIBSECURE? libsecure = new();

            while (inputIndex < inputLength)
            {
                int blockSize = Math.Min(8, inputLength - inputIndex);
                byte[] block = new byte[blockSize];
                Buffer.BlockCopy(inputArray, inputIndex, block, 0, blockSize);

                TaskCompletionSource<byte[]> tcs = new();

                await Task.Run(() =>
                {
                    tcs.SetResult(libsecure.InitiateLibSecureXTEACTRBuffer(block, Key, IV, blockSize) ?? new byte[blockSize]); // Null Bytes if failed.
                });

                toolsimpl.IncrementIVBytes(IV, 1);

                await semaphore.WaitAsync();

                byte[] taskResult = await tcs.Task;
                Buffer.BlockCopy(taskResult, 0, output, outputIndex, blockSize);
                outputIndex += blockSize;
                semaphore.Release();

                inputIndex += blockSize;
            }

            semaphore.Dispose();
            toolsimpl = null;
            libsecure = null;

            return output;
        }

        public static void aesecbDecrypt(
          byte[] key,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.ECB;
            PaddingMode padding = PaddingMode.None;
            int decryptMode = DECRYPT_MODE;
            crypto(key, mode, padding, null, decryptMode, i, inOffset, len, o, outOffset);
        }

        public static void aesecbEncrypt(
          byte[] key,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.ECB;
            PaddingMode padding = PaddingMode.None;
            int encryptMode = ENCRYPT_MODE;
            crypto(key, mode, padding, null, encryptMode, i, inOffset, len, o, outOffset);
        }

        public static void aescbcDecrypt(
          byte[] key,
          byte[] iv,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.CBC;
            PaddingMode padding = PaddingMode.None;
            int decryptMode = DECRYPT_MODE;
            crypto(key, mode, padding, iv, decryptMode, i, inOffset, len, o, outOffset);
        }

        private static void crypto(
          byte[] key,
          CipherMode mode,
          PaddingMode padding,
          byte[]? iv,
          int opMode,
          byte[] i,
          int inOffset,
          int len,
          byte[] o,
          int outOffset)
        {
            try
            {
                Aes rijndaelManaged = Aes.Create();
                rijndaelManaged.Padding = padding;
                rijndaelManaged.Mode = mode;
                rijndaelManaged.KeySize = 128;
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.Key = key;
                if (iv != null)
                    rijndaelManaged.IV = iv;
                byte[]? src = null;
                if (opMode == DECRYPT_MODE)
                    src = rijndaelManaged.CreateDecryptor().TransformFinalBlock(i, inOffset, len);
                else if (opMode == ENCRYPT_MODE)
                    src = rijndaelManaged.CreateEncryptor().TransformFinalBlock(i, inOffset, len);
                else
                    fail("NOT SUPPORTED OPMODE");
                ConversionUtils.arraycopy(src, 0, o, outOffset, len);
            }
            catch (Exception ex)
            {
                fail(ex.Message);
            }
        }

        public static byte[] CMAC128(byte[] key, byte[] i, int inOffset, int len)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            calculateSubkey(key, numArray1, numArray2);
            byte[] numArray3 = new byte[16];
            byte[] numArray4 = new byte[16];
            int srcPos = inOffset;
            int length;
            for (length = len; length > 16; length -= 16)
            {
                ConversionUtils.arraycopy(i, srcPos, numArray3, 0L, 16);
                XOR(numArray3, numArray3, numArray4);
                aesecbEncrypt(key, numArray3, 0, numArray4, 0, numArray3.Length);
                srcPos += 16;
            }
            byte[] numArray5 = new byte[16];
            ConversionUtils.arraycopy(i, srcPos, numArray5, 0L, length);
            if (length == 16)
            {
                XOR(numArray5, numArray5, numArray4);
                XOR(numArray5, numArray5, numArray1);
            }
            else
            {
                numArray5[length] = 128;
                XOR(numArray5, numArray5, numArray4);
                XOR(numArray5, numArray5, numArray2);
            }
            aesecbEncrypt(key, numArray5, 0, numArray4, 0, numArray5.Length);
            return numArray4;
        }

        public static void XOR(byte[] output, byte[] inputA, byte[] inputB)
        {
            for (int index = 0; index < inputA.Length; ++index)
                output[index] = (byte)(inputA[index] ^ (uint)inputB[index]);
        }

        public ulong Sha1toNonce(byte[] digest)
        {
            ulong v1 = 0UL;
            if (digest != null && digest.Length >= 8)
                v1 = BitConverter.ToUInt64(digest, 0);
            return v1;
        }

        public byte[] ApplyPaddingPrefix(byte[] filebytes) // Before you say anything, this is an actual Home Feature...
        {
            byte[] returnbytes = MiscUtils.Combinebytearay(new byte[] { 0x00, 0x00, 0x00, 0x01 }, filebytes);
            return returnbytes;
        }

        public byte[] RemovePaddingPrefix(byte[] fileBytes) // For Encryption Proxy, XTEA Proxy and INF files.
        {
            if (fileBytes[0] == 0x00 && fileBytes[1] == 0x00 && fileBytes[2] == 0x00 && fileBytes[3] == 0x01)
            {
                byte[] destinationArray = new byte[fileBytes.Length - 4]; // New array size after removing 4 elements

                // Copy the portion of the source array starting from index 4 to the destination array
                Array.Copy(fileBytes, 4, destinationArray, 0, destinationArray.Length);

                return destinationArray;
            }
            else
                return fileBytes;
        }

        public byte[]? Crypt_Decrypt(byte[] fileBytes, byte[] IVA, int blockSize)
        {
            if (IVA != null && IVA.Length >= blockSize && (blockSize == 16 || blockSize == 8))
            {
                StringBuilder? hexStr = new();
                LIBSECURE? libsecure = new();
                byte[]? returnstring = null;
                int i = blockSize; // Start index for processing.

                while (i <= IVA.Length)
                {
                    byte[] ivBlk = new byte[blockSize];
                    Array.Copy(IVA, i - blockSize, ivBlk, 0, ivBlk.Length);

                    byte[] block = new byte[blockSize];
                    int blockLength = Math.Min(blockSize, fileBytes.Length - (i - 8)); // Determine the block length, considering remaining bytes.
                    Array.Copy(fileBytes, i - blockSize, block, 0, blockLength);

                    // If the block length is less than blockSize, pad with ISO97971 bytes.
                    if (blockLength < blockSize)
                    {
                        int BytesToFill = blockSize - blockLength;

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

                        string hexresult = libsecure.MemXOR(MiscUtils.ByteArrayToHexString(ivBlk), MiscUtils.ByteArrayToHexString(block), blockSize);
                        hexStr.Append(hexresult.Substring(0, hexresult.Length - BytesToFill * 2)); // Pemdas rule necessary, and we double size because we work with bytes in a string.
                    }
                    else
                        hexStr.Append(libsecure.MemXOR(MiscUtils.ByteArrayToHexString(ivBlk), MiscUtils.ByteArrayToHexString(block), blockSize));

                    i += blockSize;
                }

                returnstring = MiscUtils.HexStringToByteArray(hexStr.ToString());

                hexStr = null;
                libsecure = null;

                return returnstring;
            }
            else
                LoggerAccessor.LogError("[AFS] - Crypt_Decrypt - No IV entered or invalid length!");

            return null;
        }

        internal struct ChunkHeader
        {
            internal byte[] GetBytes()
            {
                byte[] array = new byte[4];
                Array.Copy(BitConverter.GetBytes(SourceSize), 0, array, 2, 2);
                Array.Copy(BitConverter.GetBytes(CompressedSize), 0, array, 0, 2);
                return array;
            }

            internal static int SizeOf
            {
                get
                {
                    return 4;
                }
            }

            internal static ChunkHeader FromBytes(byte[] inData)
            {
                ChunkHeader result = default;
                byte[] array = inData;
                if (inData.Length > SizeOf)
                {
                    array = new byte[4];
                    Array.Copy(inData, array, 4);
                }
                result.SourceSize = BitConverter.ToUInt16(array, 2);
                result.CompressedSize = BitConverter.ToUInt16(array, 0);
                return result;
            }

            internal ushort SourceSize;

            internal ushort CompressedSize;
        }
    }
}
