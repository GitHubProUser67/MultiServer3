using CustomLogger;
using System.Numerics;
using System.Security.Cryptography;
using EndianTools;
using System;
using CyberBackendLibrary.Crypto;
using CyberBackendLibrary.Extension;

namespace HomeTools.Crypto
{
    public class ToolsImpl
    {
        public static readonly string base64DefaultSharcKey = "L1ztpjqaZywDTBLh5CX6gRYWrhzmbeuVt+a/IUBHAtw=";

        public static readonly string base64CDNKey1 = "8243a3b10f1f1660a7fc934aac263c9c5161092dc25=";

        public static readonly string base64CDNKey2 = "8b9qT7u6XQ7Sf0GKSIivMEeG7NROLTZGgNtN8iI6n1Y=";

        public static readonly byte[] DefaultKey = new byte[]
        {
            0x80, 0x6d, 0x79, 0x16, 0x23, 0x42, 0xa1, 0x0e,
            0x8f, 0x78, 0x14, 0xd4, 0xf9, 0x94, 0xa2, 0xd1,
            0x74, 0x13, 0xfc, 0xa8, 0xf6, 0xe0, 0xb8, 0xa4,
            0xed, 0xb9, 0xdc, 0x32, 0x7f, 0x8b, 0xa7, 0x11
        };
        public static readonly byte[] SignatureKey = new byte[]
        {
            0xef, 0x8c, 0x7d, 0xe8, 0xe5, 0xd5, 0xd6, 0x1d,
            0x6a, 0xaa, 0x5a, 0xca, 0xf7, 0xc1, 0x6f, 0xc4,
            0x5a, 0xfc, 0x59, 0xe4, 0x8f, 0xe6, 0xc5, 0x93,
            0x7e, 0xbd, 0xff, 0xc1, 0xe3, 0x99, 0x9e, 0x62
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
        public static readonly byte[] HDKContentCreationSignatureKey = new byte[] // Insert "IT'S A TRAP" here, just made to make HDK Content Creation builds
        {                                                                         // not decrypt the EncryptionProxy Signature Header.
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F,
            0xBA, 0x98, 0x76, 0x54, 0x32, 0x10, 0x5C, 0x1F
        };
        public static readonly byte[] BlowfishKey = new byte[] // Seems to be the equivalent of "DO NOT SEND THE SIGNAL" for the default key
        {                                                      // on HDK Content Creation builds.
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB,
            0xF1, 0xC5, 1, 0x23, 0x45, 0x67, 0x89, 0xAB
        };

        public static readonly byte[] MetaDataV1IV = new byte[] { 0x2a, 0xa7, 0xcb, 0x49, 0x9f, 0xa1, 0xbd, 0x81 };

        public static readonly byte[] TicketListV0IV = new byte[] { 0x30, 0x4B, 0x10, 0x3D, 0x46, 0x77, 0xAD, 0x84 };

        public static readonly byte[] TicketListV1IV = new byte[] { 0xc7, 0x96, 0x79, 0xe5, 0x79, 0x99, 0x9f, 0xbf };

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

        public static byte[] ProcessXTEAProxyBlocks(byte[] inputArray, byte[] Key, byte[] IV)
        {
            int inputIndex = 0;
            int inputLength = inputArray.Length;
            byte[] block = new byte[8];
            byte[]? output = new byte[inputLength];

            while (inputIndex < inputLength)
            {
                int blockSize = Math.Min(8, inputLength - inputIndex);
                if (blockSize < 8)
                {
                    int difference = 8 - blockSize;
                    Buffer.BlockCopy(new byte[difference], 0, block, block.Length - difference, difference);
                }
                Buffer.BlockCopy(inputArray, inputIndex, block, 0, blockSize);
                byte[]? taskResult = LIBSECURE.InitiateXTEABuffer(block, Key, IV, "CTR");
                if (taskResult == null) // We failed so we send original file back.
                    return inputArray;
                if (taskResult.Length < blockSize)
                    Buffer.BlockCopy(taskResult, 0, output, inputIndex, taskResult.Length);
                else
                    Buffer.BlockCopy(taskResult, 0, output, inputIndex, blockSize);
                IncrementIVBytes(IV, 1);
                inputIndex += blockSize;
            }

            return output;
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
            return DataUtils.CombineByteArray(new byte[] { 0x01, 0x00, 0x00, 0x00 }, filebytes);
        }

        public static byte[] ApplyLittleEndianPaddingPrefix(byte[] filebytes) // Before you say anything, this is an actual Home Feature...
        {
            return DataUtils.CombineByteArray(new byte[] { 0x00, 0x00, 0x00, 0x01 }, filebytes);
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

        internal struct ChunkHeader
        {
            internal byte[] GetBytes()
            {
                byte[] array = new byte[4];
                Array.Copy(BitConverter.GetBytes((!BitConverter.IsLittleEndian) ? EndianUtils.ReverseUshort(SourceSize) : SourceSize), 0, array, 2, 2);
                Array.Copy(BitConverter.GetBytes((!BitConverter.IsLittleEndian) ? EndianUtils.ReverseUshort(CompressedSize): CompressedSize), 0, array, 0, 2);
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

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(array);

                result.SourceSize = BitConverter.ToUInt16(array, 2);
                result.CompressedSize = BitConverter.ToUInt16(array, 0);
                return result;
            }

            internal ushort SourceSize;

            internal ushort CompressedSize;
        }
    }
}
