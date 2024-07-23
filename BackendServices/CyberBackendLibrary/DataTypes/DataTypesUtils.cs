using System.Linq;
using System.IO;
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace CyberBackendLibrary.DataTypes
{
    public class DataTypesUtils
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static readonly bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT
            || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;

        private static readonly uint[] _lookup32Unsafe = CreateLookup32Unsafe();
        private unsafe static readonly uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_lookup32Unsafe, GCHandleType.Pinned).AddrOfPinnedObject();

        private static uint[] CreateLookup32Unsafe()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                if (BitConverter.IsLittleEndian)
                    result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
                else
                    result[i] = ((uint)s[1]) + ((uint)s[0] << 16);
            }
            return result;
        }

        /// <summary>
        /// Transform a byte array to it's hexadecimal representation.
        /// <para>Obtenir un tableau de bytes dans sa représentation hexadecimale.</para>
        /// <param name="bytes">The byte array to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public unsafe static string ByteArrayToHexString(byte[] bytes)
        {
            uint* lookupP = _lookup32UnsafeP;
            char[] result = new char[bytes.Length * 2];
            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result)
            {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < bytes.Length; i++)
                {
                    resultP2[i] = lookupP[bytesP[i]];
                }
            }
            return new string(result);
        }

        /// <summary>
        /// Check if 2 byte arrays are strictly identical.
        /// <para>Savoir si 2 tableaux de bytes sont strictement identiques.</para>
        /// <param name="b1">The left array.</param>
        /// <param name="b2">The right array.</param>
        /// </summary>
        /// <returns>A boolean.</returns>
        public static bool AreArraysIdentical(byte[] b1, byte[] b2)
        {
            if (IsWindows)
                // Validate buffers are the same length.
                // This also ensures that the count does not exceed the length of either buffer.  
                return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;

            return b1.SequenceEqual(b2);
        }

        /// <summary>
        /// Verify is the string is in base64 format.
        /// <para>Vérifie si un string est en format base64.</para>
        /// </summary>
        /// <param name="base64">The base64 string.</param>
        /// <returns>A boolean.</returns>
        public static bool IsBase64String(string base64)
        {
            return Convert.TryFromBase64String(base64, new Span<byte>(new byte[base64.Length]), out _);
        }

        /// <summary>
        /// Convert a hex-formatted string to byte array.
        /// <para>Convertir une représentation hexadécimal en tableau de bytes.</para>
        /// </summary>
        /// <param name="hex">A string looking like "300D06092A864886F70D0101050500".</param>
        /// <returns>A byte array.</returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            string cleanedRequest = hex.Replace(" ", string.Empty).Replace("\n", string.Empty);

            if (cleanedRequest.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[cleanedRequest.Length >> 1];

            for (int i = 0; i < cleanedRequest.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(cleanedRequest[i << 1]) << 4) + (GetHexVal(cleanedRequest[(i << 1) + 1])));
            }

            return arr;
        }

        /// <summary>
        /// Combines 2 bytes array in one unique byte array.
        /// <para>Combiner 2 tableaux de bytes en un seul tableau de bytes.</para>
        /// </summary>
        /// <param name="first">The first byte array, which represents the left.</param>
        /// <param name="second">The second byte array, which represents the right.</param>
        /// <returns>A byte array.</returns>
        public static byte[] CombineByteArray(byte[] first, byte[]? second)
        {
            if (second == null)
                return first;

            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        /// <summary>
        /// Combines a byte array with an array of byte array to a unique byte array.
        /// <para>Combiner un tableau de bytes avec un tableau de tableaux de bytes en un seul tableau de bytes.</para>
        /// </summary>
        /// <param name="first">The first byte array, which represents the left.</param>
        /// <param name="second">The array of byte array, which represents the right.</param>
        /// <returns>A byte array.</returns>
        public static byte[] CombineByteArrays(byte[] first, byte[][]? second)
        {
            if (second == null || second.Length == 0)
                return first;

            byte[] result = new byte[first.Length + second.Sum(arr => arr.Length)];

            Buffer.BlockCopy(first, 0, result, 0, first.Length);

            int offset = first.Length;
            foreach (byte[] array in second)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        /// <summary>
        /// Reads a fragment of a file with a given indicator.
        /// <para>Lire un fragment de fichier avec un indicateur explicite.</para>
        /// </summary>
        /// <param name="filePath">The path of the desired file.</param>
        /// <param name="bytesToRead">The amount of desired fragment data.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReadSmallFileChunck(string filePath, int bytesToRead)
        {
            if (bytesToRead <= 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToRead), "[DataTypesUtils] - ReadSmallFileChunck() - Number of bytes to read must be greater than zero.");

            int bytesRead = 0;
            Span<byte> result = new byte[bytesToRead];

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using BinaryReader reader = new BinaryReader(fileStream);
                bytesRead = reader.Read(result);

                // If the file is less than 'bytesToRead', pad with null bytes
                if (bytesRead < bytesToRead)
                    result[bytesRead..].Fill(0);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Adds an element to a double string array.
        /// <para>Ajoute un élément à une liste double de strings.</para>
        /// </summary>
        /// <param name="original">The original double array.</param>
        /// <param name="bytesToRead">The new array to add.</param>
        /// <returns>A double array of strings.</returns>
        public static string[][] AddElement(string[][] original, string[] newElement)
        {
            int newSize = original.Length + 1;
            string[][] newArray = new string[newSize][];
            for (int i = 0; i < original.Length; i++)
            {
                newArray[i] = original[i];
            }
            newArray[newSize - 1] = newElement;
            return newArray;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        /// <summary>
        /// Finds a sequence of bytes within a byte array.
        /// <para>Trouve une séquence de bytes dans un tableau de bytes.</para>
        /// </summary>
        /// <param name="buffer">The array in which we search for the sequence.</param>
        /// <param name="searchPattern">The byte array sequence to find.</param>
        /// <param name="offset">The offset from where we start our research.</param>
        /// <returns>A int (-1 if not found).</returns>
        public static int FindBytePattern(byte[] buffer, byte[] searchPattern, int offset = 0)
        {
            int found = -1;

            if (buffer.Length > 0 && searchPattern.Length > 0 && offset <= buffer.Length - searchPattern.Length && buffer.Length >= searchPattern.Length)
            {
                for (int i = offset; i <= buffer.Length - searchPattern.Length; i++)
                {
                    if (buffer[i] == searchPattern[0])
                    {
                        if (buffer.Length > 1)
                        {
                            bool matched = true;
                            for (int y = 1; y <= searchPattern.Length - 1; y++)
                            {
                                if (buffer[i + y] != searchPattern[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            if (matched)
                            {
                                found = i;
                                break;
                            }
                        }
                        else
                        {
                            found = i;
                            break;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Finds a sequence of bytes within a byte array.
        /// <para>Trouve une séquence de bytes dans un tableau de bytes.</para>
        /// </summary>
        /// <param name="buffer">The Span byte in which we search for the sequence.</param>
        /// <param name="searchPattern">The Span byte sequence to find.</param>
        /// <param name="offset">The offset from where we start our research.</param>
        /// <returns>A int (-1 if not found).</returns>
        public static int FindBytePattern(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> searchPattern, int offset = 0)
        {
            if (searchPattern.IsEmpty || buffer.Length < searchPattern.Length || offset > buffer.Length - searchPattern.Length)
                return -1;

            for (int i = offset; i < buffer.Length - searchPattern.Length + 1; i++)
            {
                if (buffer[i] == searchPattern[0] && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
            }

            return -1;
        }

        public static bool IsObjectLocked(object obj)
        {
            // Try to acquire the lock
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(obj, ref lockTaken);
                return !lockTaken;
            }
            finally
            {
                // Release the lock if it was taken
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }
    }
}
