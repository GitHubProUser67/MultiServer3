using System.Text;
using System.Linq;
using System.IO;
using System;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif

namespace CyberBackendLibrary.DataTypes
{
    public class DataTypesUtils
    {
        /// <summary>
        /// Transform a byte array to it's hexadecimal representation.
        /// <para>Obtenir un tableau de bytes dans sa représentation hexadecimale.</para>
        /// <param name="byteArray">The byte array to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);

            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Check if 2 byte arrays are strictly identical.
        /// <para>Savoir si 2 tableaux de bytes sont strictement identiques.</para>
        /// <param name="arr1">The left array.</param>
        /// <param name="arr2">The right array.</param>
        /// </summary>
        /// <returns>A boolean.</returns>
        public static bool AreArraysIdentical(byte[] arr1, byte[] arr2)
        {
            // Check if the length of both arrays is the same
            if (!AreIntegersIdentical(arr1.Length, arr2.Length))
                return false;

            // Compare each element in the arrays
            for (int i = 0; i < arr1.Length; i++)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Avx2.IsSupported && Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, arr1[i]), Vector256<byte>.Zero.WithElement(0, arr2[i]))) == 0)
                    return false;
                else if (Sse2.IsSupported && Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, arr1[i]), Vector128<byte>.Zero.WithElement(0, arr2[i]))) == 0)
                    return false;
                else if (arr1[i] != arr2[i])
                    return false;
#else
                if (arr1[i] != arr2[i])
                    return false;
#endif
            }

            // If all elements are identical, return true
            return true;
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
            //copypasted from:
            //https://social.msdn.microsoft.com/Forums/en-US/851492fa-9ddb-42d7-8d9a-13d5e12fdc70/convert-from-a-hex-string-to-a-byte-array-in-c?forum=aspgettingstarted
            string cleanedRequest = hex.Replace(" ", string.Empty).Replace("\n", string.Empty);
            return Enumerable.Range(0, cleanedRequest.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(cleanedRequest.Substring(x, 2), 16))
                                 .ToArray();
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
#if NETCOREAPP3_0_OR_GREATER
                    if (Avx2.IsSupported)
                    {
                        if (Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, buffer[i]), Vector256<byte>.Zero.WithElement(0, searchPattern[0]))) != 0)
                        {
                            if (buffer.Length > 1)
                            {
                                bool matched = true;
                                for (int y = 1; y <= searchPattern.Length - 1; y++)
                                {
                                    if (Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, buffer[i + y]), Vector256<byte>.Zero.WithElement(0, searchPattern[y]))) == 0)
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
                    else if (Sse2.IsSupported)
                    {
                        if (Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, buffer[i]), Vector128<byte>.Zero.WithElement(0, searchPattern[0]))) != 0)
                        {
                            if (buffer.Length > 1)
                            {
                                bool matched = true;
                                for (int y = 1; y <= searchPattern.Length - 1; y++)
                                {
                                    if (Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, buffer[i + y]), Vector128<byte>.Zero.WithElement(0, searchPattern[y]))) == 0)
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
                    else if (buffer[i] == searchPattern[0])
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
#else
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
#endif
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
#if NETCOREAPP3_0_OR_GREATER
                if (Avx2.IsSupported && Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, buffer[i]), Vector256<byte>.Zero.WithElement(0, searchPattern[0]))) != 0 && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
                else if (Sse2.IsSupported && Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, buffer[i]), Vector128<byte>.Zero.WithElement(0, searchPattern[0]))) != 0 && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
                else if (buffer[i] == searchPattern[0] && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
#else
                if (buffer[i] == searchPattern[0] && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
#endif
            }

            return -1;
        }

        public static bool AreIntegersIdentical(int a, int b)
        {
#if NETCOREAPP3_0_OR_GREATER
            // With SIMD, Check if the comparison results are all 1's (indicating equality)
            if (Avx2.IsSupported)
                return Avx2.CompareEqual(Vector256.Create(a), Vector256.Create(b)).Equals(Vector256<int>.AllBitsSet);
            else if (Sse2.IsSupported)
                return Sse2.CompareEqual(Vector128.Create(a), Vector128.Create(b)).Equals(Vector128<int>.AllBitsSet);
#endif
            return a == b;
        }
    }
}
