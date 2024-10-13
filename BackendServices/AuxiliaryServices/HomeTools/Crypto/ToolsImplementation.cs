using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using NetworkLibrary.Extension;
using HashLib;

namespace HomeTools.Crypto
{
    public class ToolsImplementation
    {
        public static readonly string base64DefaultSharcKey = "L1ztpjqaZywDTBLh5CX6gRYWrhzmbeuVt+a/IUBHAtw=";

        public static readonly string base64CDNKey1 = "8243a3b10f1f1660a7fc934aac263c9c5161092dc25=";

        public static readonly string base64CDNKey2 = "8b9qT7u6XQ7Sf0GKSIivMEeG7NROLTZGgNtN8iI6n1Y=";

        public static readonly byte[] BlowfishKey = new byte[]
        {
            0x80, 0x6d, 0x79, 0x16, 0x23, 0x42, 0xa1, 0x0e,
            0x8f, 0x78, 0x14, 0xd4, 0xf9, 0x94, 0xa2, 0xd1,
            0x74, 0x13, 0xfc, 0xa8, 0xf6, 0xe0, 0xb8, 0xa4,
            0xed, 0xb9, 0xdc, 0x32, 0x7f, 0x8b, 0xa7, 0x11
        };
        public static readonly byte[] BetaBlowfishKey = new byte[]
        {
            0x81, 0x61, 0xCB, 0x66, 0xEE, 0x70, 0xCD, 0x5E,
            0x80, 0x8D, 0xA5, 0xB5, 0xBC, 0x34, 0x4D, 0x74,
            0x71, 0x04, 0x4E, 0xC3, 0x6A, 0xFC, 0x3B, 0x24,
            0x1A, 0x03, 0xCE, 0xFD, 0x9B, 0x63, 0x0E, 0xBD
        };
        public static readonly byte[] HDKBlowfishKey = new byte[]
        {
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB
        };
        public static readonly byte[] SignatureKey = new byte[]
        {
            0xef, 0x8c, 0x7d, 0xe8, 0xe5, 0xd5, 0xd6, 0x1d,
            0x6a, 0xaa, 0x5a, 0xca, 0xf7, 0xc1, 0x6f, 0xc4,
            0x5a, 0xfc, 0x59, 0xe4, 0x8f, 0xe6, 0xc5, 0x93,
            0x7e, 0xbd, 0xff, 0xc1, 0xe3, 0x99, 0x9e, 0x62
        };
        public static readonly byte[] BetaSignatureKey = new byte[]
        {
            0x84, 0xE1, 0x3E, 0x1C, 0x26, 0xDD, 0xB5, 0xE9,
            0xB4, 0x13, 0x1B, 0x67, 0xB3, 0x71, 0xD2, 0xD7,
            0x3E, 0xF5, 0x24, 0x26, 0xDC, 0x83, 0x85, 0x7D,
            0x7D, 0x11, 0xF6, 0x2B, 0xFC, 0x2D, 0xEF, 0xC1
        };
        public static readonly byte[] HDKSignatureKey = new byte[]
        {
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F
        };
        public static readonly byte[] contentkey = new byte[]
        {
            47, 92, 237, 166, 58, 154, 103, 44,
            3, 76, 18, 225, 228, 37, 250, 129,
            22, 22, 174, 28, 230, 109, 235, 149,
            183, 230, 191, 33, 64, 71, 2, 220
        };
        public static readonly byte[] EncryptionKey = new byte[]
        {
            0xCF, 0x5B, 0xC6, 0x44, 0x0B, 0x9F, 0x0E, 0xB0,
            0x52, 0xB1, 0x0A, 0xC0, 0x2B, 0x0C, 0xD5, 0x07,
            0x02, 0xCB, 0x44, 0x9F, 0x4F, 0x2A, 0xBA, 0x6A,
            0x4B, 0xA4, 0xFF, 0x02, 0x5C, 0x5F, 0xAC, 0x15
        };
        public static readonly byte[] MetaDataV1Key = new byte[]
        {
            0x8B, 0x41, 0xA7, 0xDE, 0x47, 0xA0, 0xD4, 0x45,
            0xE2, 0xA5, 0x90, 0x34, 0x3C, 0xD9, 0xA8, 0xB5,
            0x69, 0x5E, 0xFA, 0xD9, 0x97, 0x32, 0xEC, 0x56,
            0xB, 0x31, 0xE8, 0x5A, 0xD1, 0x85, 0x7C, 0x89
        };
        public static readonly byte[] TicketListV0Key = new byte[]
        {
            4, 0x6B, 0xD9, 0x2A, 0x10, 0xAC, 0x6A, 0x3F,
            0xEB, 0x5C, 0xB7, 0x6D, 0x2C, 6, 0xCB, 0xA6,
            0xF, 0x4E, 0x64, 0x36, 0x63, 0x7B, 0xFA, 0x51,
            0x97, 0xE6, 0x2A, 0xF0, 0x76, 0xDA, 0x9E, 0xA2
        };
        public static readonly byte[] TicketListV1Key = new byte[]
        {
            0x5B, 0xD6, 0xC7, 0xC8, 0xD6, 0x76, 0x37, 0x9E,
            0x8F, 0x54, 0xC1, 0x56, 0xE3, 0x88, 0x87, 0x8D,
            0x98, 0x6C, 0xC, 0x3E, 0x4D, 0x5D, 0x1C, 0x4B,
            0x32, 2, 0xB1, 0xE5, 0x2F, 0x81, 0x35, 0x44
        };
        public static readonly byte[] ProfanityFilterCacheKey = new byte[]
        {
            0xb7, 0x71, 0x29, 0x23, 0x24, 0xa9, 0xc0, 0xa3,
            0x86, 0x02, 0x01, 0xa2, 0x06, 0x6c, 0xce, 0x77,
            0x1d, 0x34, 0x62, 0xfb, 0xc2, 0x97, 0x52, 0x67,
            0xc5, 0x23, 0x08, 0x3c, 0xfb, 0x0d, 0x9b, 0x34
        };
        public static readonly byte[] MQDiskCacheKey = new byte[]
        {
            0x42, 0x01, 0xc8, 0xcc, 0xf4, 0xb7, 0x35, 0x4b,
            0xca, 0x72, 0x2d, 0xac, 0x0a, 0x50, 0xe6, 0x56,
            0x5d, 0xb1, 0xd9, 0x05, 0x9f, 0x22, 0xb6, 0x9c,
            0xd1, 0x0b, 0xb2, 0x7e, 0x0d, 0xad, 0x0e, 0x37
        };

        public static readonly byte[] MetaDataV1IV = new byte[] { 0x2a, 0xa7, 0xcb, 0x49, 0x9f, 0xa1, 0xbd, 0x81 };

        public static readonly byte[] TicketListV0IV = new byte[] { 0x30, 0x4B, 0x10, 0x3D, 0x46, 0x77, 0xAD, 0x84 };

        public static readonly byte[] TicketListV1IV = new byte[] { 0xc7, 0x96, 0x79, 0xe5, 0x79, 0x99, 0x9f, 0xbf };

        public static readonly byte[] ProfanityFilterCacheIV = new byte[] { 0xcc, 0x1b, 0x4f, 0x6a, 0x54, 0xa2, 0xab, 0x8c };

        public static readonly byte[] MQDiskCacheIV = new byte[] { 0x64, 0xbc, 0x9f, 0xe2, 0x2a, 0xe4, 0x04, 0xd8 };

        public static ulong BuildSignatureIv(int fileSize, int compressedSize, int dataStart, int userData)
        {
            return (ulong)fileSize << 0x30 | (ulong)compressedSize << 0x20 & 0xFFFF00000000UL | (ulong)dataStart << 0xE & 0xFFFF0000UL | (ushort)userData;
        }

        public static void IncrementIVBytes(byte[] byteArray, int increment)
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

        public static async Task<byte[]> ProcessXTEAProxyAsync(byte[] inData, byte[] KeyBytes, byte[] IV)
        {
            const byte xteaBlockSize = 8;
            int chunkIndex = 0;
            int inputLength = inData.Length;
            List<KeyValuePair<(int, int), Task<byte[]>>> xteaTasks = new List<KeyValuePair<(int, int), Task<byte[]>>>();

            using (MemoryStream memoryStream = new MemoryStream(inData))
            {
                while (memoryStream.Position < memoryStream.Length)
                {
                    byte[] block = new byte[xteaBlockSize];
                    byte[] blockIV = (byte[])IV.Clone();
                    int blockSize = Math.Min(xteaBlockSize, inputLength - chunkIndex);
                    if (blockSize < xteaBlockSize)
                    {
                        int difference = xteaBlockSize - blockSize;
                        Buffer.BlockCopy(new byte[difference], 0, block, block.Length - difference, difference);
                    }
                    memoryStream.Read(block, 0, blockSize);
                    xteaTasks.Add(new KeyValuePair<(int, int), Task<byte[]>>((chunkIndex, blockSize), LIBSECURE.InitiateXTEABufferAsync(block, KeyBytes, blockIV, "CTR")));
                    IncrementIVBytes(IV, 1);
                    chunkIndex += blockSize;
                }
            }

            using (MemoryStream memoryStream = new MemoryStream(inData.Length))
            {
                foreach (var result in xteaTasks.OrderBy(kv => kv.Key.Item1))
                {
                    try
                    {
                        // Await each decryption task
                        byte[] decryptedChunk = await result.Value.ConfigureAwait(false);
                        if (decryptedChunk == null) // We failed so we send original file back.
                            return inData;
                        if (decryptedChunk.Length < result.Key.Item2)
                            memoryStream.Write(decryptedChunk, 0, decryptedChunk.Length);
                        else
                            memoryStream.Write(decryptedChunk, 0, result.Key.Item2);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"[ToolsImplementation] - ProcessXTEAProxyAsync: Error during decryption at chunk {result.Key}", ex);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static ulong Sha1toNonce(byte[] digest)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(digest);

            ulong v1 = 0UL;
            if (digest != null && digest.Length >= 8)
                v1 = BitConverter.ToUInt64(digest, 0);
            return v1;
        }

        public static byte[] ApplyBigEndianPaddingPrefix(byte[] filebytes) // Before you say anything, this is an actual Home Feature...
        {
            return OtherExtensions.CombineByteArray(new byte[] { 0x01, 0x00, 0x00, 0x00 }, filebytes);
        }

        public static byte[] ApplyLittleEndianPaddingPrefix(byte[] filebytes) // Before you say anything, this is an actual Home Feature...
        {
            return OtherExtensions.CombineByteArray(new byte[] { 0x00, 0x00, 0x00, 0x01 }, filebytes);
        }

        public static byte[] RemovePaddingPrefix(byte[] fileBytes) // For Encryption Proxy, TicketList and INF files.
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
    }
}
