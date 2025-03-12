using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Tpm2Lib;

namespace NetworkLibrary.Extension
{
    public static class ByteUtils
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

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
        /// Transform a Span byte array to it's hexadecimal representation.
        /// <para>Obtenir un tableau de Span bytes dans sa représentation hexadecimale.</para>
        /// <param name="bytes">The Span byte array to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public unsafe static string ToHexString(this Span<byte> bytes)
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
        /// Transform a byte array to it's hexadecimal representation.
        /// <para>Obtenir un tableau de bytes dans sa représentation hexadecimale.</para>
        /// <param name="bytes">The byte array to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public unsafe static string ToHexString(this byte[] bytes)
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
        /// Convert UTF-8 byte array to array of uint, each containing 4 chars to be manipulated
        /// </summary>
        /// <param name="s"></param>
        /// <param name="LittleEndian">If true, use little-endian encoding; if false, use big-endian encoding</param>
        /// <returns>Array of uint representing the byte data</returns>
        public static uint[] ToUint32(this byte[] s, bool LittleEndian = true)
        {
            // Note chars must be within ISO-8859-1 (with Unicode code-point < 256) to fit 4/uint
            uint[] l = new uint[(int)Math.Ceiling((decimal)s.Length / 4)];

            // Create an array of uint, each holding the data of 4 characters
            // If the last block is less than 4 characters in length, fill with ascii null values
            for (int i = 0; i < l.Length; i++)
            {
                byte b0 = (i * 4) < s.Length ? s[i * 4] : (byte)0;
                byte b1 = (i * 4 + 1) < s.Length ? s[i * 4 + 1] : (byte)0;
                byte b2 = (i * 4 + 2) < s.Length ? s[i * 4 + 2] : (byte)0;
                byte b3 = (i * 4 + 3) < s.Length ? s[i * 4 + 3] : (byte)0;

                if (LittleEndian)
                {
                    // Little-endian: Least significant byte first
                    l[i] = (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
                }
                else
                {
                    // Big-endian: Most significant byte first
                    l[i] = (uint)(b3 | (b2 << 8) | (b1 << 16) | (b0 << 24));
                }
            }

            return l;
        }

        /// <summary>
        /// Check if 2 byte arrays are strictly identical.
        /// <para>Savoir si 2 tableaux de bytes sont strictement identiques.</para>
        /// <param name="b1">The left array.</param>
        /// <param name="b2">The right array.</param>
        /// </summary>
        /// <returns>A boolean.</returns>
        public static bool EqualsTo(this byte[] b1, byte[] b2)
        {
            bool isb1Null = b1 == null;
            bool isb2Null = b2 == null;
            
            if (isb1Null && isb2Null)
                return true;
            else if (isb1Null || isb2Null)
                return false;

            if (Windows.Win32API.IsWindows)
                // Validate buffers are the same length.
                // This also ensures that the count does not exceed the length of either buffer.  
                return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;

            int i;
            if (b1.Length == b2.Length)
            {
                i = 0;
                while (i < b1.Length && (b1[i] == b2[i]))
                {
                    i++;
                }
                if (i == b1.Length)
                    return true;
            }

            return false;
        }

        public static byte[] Trim(this byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }

        public static byte[] GenerateRandomBytes(ushort size)
        {
            Tpm2 _tpm = null;

            try
            {
                TbsDevice _crypto_device = new TbsDevice();
                _crypto_device.Connect();
                _tpm = new Tpm2(_crypto_device);

                return _tpm.GetRandom(size);
            }
            catch
            {
                // Fallback to classic .NET version.
            }
            finally
            {
                if (_tpm != null) _tpm.Dispose();
            }

            byte[] result = new byte[size];

#if NETCOREAPP2_0_OR_GREATER
            RandomNumberGenerator.Fill(result);
#else
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(result);
#endif

            return result;
        }

        /// <summary>
        /// Combines 2 bytes array in one unique byte array.
        /// <para>Combiner 2 tableaux de bytes en un seul tableau de bytes.</para>
        /// </summary>
        /// <param name="first">The first byte array, which represents the left.</param>
        /// <param name="second">The second byte array, which represents the right.</param>
        /// <returns>A byte array.</returns>
        public static byte[] CombineByteArray(byte[] first, byte[] second)
        {
            bool isfirstNull = first == null;
            bool issecondNull = second == null;
            
            if (isfirstNull && issecondNull)
                return null;
            else if (issecondNull || second.Length == 0)
            {
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }
            else if (isfirstNull || first.Length == 0)
            {
                int sizeOfArray = second.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(second, 0, copy, 0, sizeOfArray);
                return copy;
            }

            bool exceptionThrown = false;
            int totalLength = first.Length + second.Length;
#if NET6_0_OR_GREATER
            if (totalLength > Array.MaxLength || totalLength < 0)
#else
            if (totalLength > 0X7FFFFFC7 || totalLength < 0)
#endif
            {
                // Return the first array if total length exceeds limits
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }

            byte[] bytes = new byte[totalLength];

            // Combine first, and second arrays
            Task t = Task.Run(() => { Array.Copy(first, 0, bytes, 0, first.Length); });
            try
            {
                Array.Copy(second, 0, bytes, first.Length, second.Length);
            }
            catch
            {
                exceptionThrown = true;
                throw;
            }
            finally
            {
                try
                {
                    t.Wait();
                }
                catch
                {
                    // Don't assert if we already thrown an exception.
                    if (!exceptionThrown)
#pragma warning disable CA2219
                        throw;
#pragma warning restore
                }
                finally
                {
                    t.Dispose();
                }
            }
            return bytes;
        }

        /// <summary>
        /// Combines 3 bytes arrays into one unique byte array.
        /// <para>Combine 3 tableaux de bytes en un seul tableau de bytes.</para>
        /// </summary>
        /// <param name="first">The first byte array, which represents the leftmost part.</param>
        /// <param name="second">The second byte array, which represents the middle part.</param>
        /// <param name="third">The third byte array, which represents the rightmost part.</param>
        /// <returns>A byte array.</returns>
        public static byte[] CombineByteArrays(byte[] first, byte[] second, byte[] third)
        {
            bool isfirstNull = first == null;
            bool issecondNull = second == null;
            bool isthirdNull = third == null;

            if (isfirstNull && issecondNull && isthirdNull)
                return null;
            else if (issecondNull && isthirdNull)
            {
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }
            else if (isthirdNull && isfirstNull)
            {
                int sizeOfArray = second.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(second, 0, copy, 0, sizeOfArray);
                return copy;
            }
            else if (isfirstNull && issecondNull)
            {
                int sizeOfArray = third.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(third, 0, copy, 0, sizeOfArray);
                return copy;
            }
            else if (isfirstNull || first.Length == 0)
                return CombineByteArray(second, third);
            else if (issecondNull || second.Length == 0)
                return CombineByteArray(first, third);
            else if (isthirdNull || third.Length == 0)
                return CombineByteArray(first, second);

            bool exceptionThrown = false;
            int totalLength = first.Length + second.Length + third.Length;

#if NET6_0_OR_GREATER
            if (totalLength > Array.MaxLength || totalLength < 0)
#else
    if (totalLength > 0X7FFFFFC7 || totalLength < 0)
#endif
            {
                // Return the first array if total length exceeds limits
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }

            byte[] bytes = new byte[totalLength];

            // Combine first, second, and third arrays
            Task t1 = Task.Run(() => { Array.Copy(first, 0, bytes, 0, first.Length); });
            Task t2 = Task.Run(() => { Array.Copy(second, 0, bytes, first.Length, second.Length); });
            try
            {
                Array.Copy(third, 0, bytes, first.Length + second.Length, third.Length);
            }
            catch
            {
                exceptionThrown = true;
                throw;
            }
            finally
            {
                try
                {
                    Task.WhenAll(t1, t2).Wait();
                }
                catch
                {
                    if (!exceptionThrown)
#pragma warning disable CA2219
                        throw;
#pragma warning restore
                }
                finally
                {
                    t1.Dispose();
                    t2.Dispose();
                }
            }
            return bytes;
        }

        /// <summary>
        /// Combines a byte array with an array of byte array to a unique byte array.
        /// <para>Combiner un tableau de bytes avec un tableau de tableaux de bytes en un seul tableau de bytes.</para>
        /// </summary>
        /// <param name="first">The first byte array, which represents the left.</param>
        /// <param name="second">The array of byte array, which represents the right.</param>
        /// <returns>A byte array.</returns>
        public static byte[] CombineByteArrays(byte[] first, byte[][] second)
        {
            if (second == null || second.Length == 0)
            {
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }

            int firstLength = first?.Length ?? 0;
            int totalLength = firstLength + second.Sum(arr => arr.Length);
#if NET6_0_OR_GREATER
            if (totalLength > Array.MaxLength || totalLength < 0)
#else
            if (totalLength > 0X7FFFFFC7 || totalLength < 0)
#endif
            {
                // Return the first array if total length exceeds limits
                int sizeOfArray = first.Length;
                byte[] copy = new byte[sizeOfArray];
                Array.Copy(first, 0, copy, 0, sizeOfArray);
                return copy;
            }

            bool exceptionThrown = false;
            Task t = null;
            byte[] result = new byte[totalLength];

            if (first != null)
                t = Task.Run(() => { Array.Copy(first, 0, result, 0, first.Length); });

            // Calculate offsets for each array in `second` before the parallel operation.
            int[] offsets = new int[second.Length];
            int currentOffset = firstLength;

            for (int i = 0; i < second.Length; i++)
            {
                offsets[i] = currentOffset;
                currentOffset += second[i].Length;
            }

            try
            {
                // Perform the block copy in parallel
                Parallel.ForEach(Enumerable.Range(0, second.Length), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    Array.Copy(second[i], 0, result, offsets[i], second[i].Length);
                });

                t?.Wait();
            }
            catch
            {
                exceptionThrown = true;
                throw;
            }
            finally
            {
                if (t != null)
                {
                    if ((int)t.Status < 5)
                    {
                        try
                        {
                            t.Wait();
                        }
                        catch
                        {
                            // Don't assert if we already thrown an exception.
                            if (!exceptionThrown)
#pragma warning disable CA2219
                                throw;
#pragma warning restore
                        }
                        finally
                        {
                            t.Dispose();
                        }
                    }
                    else
                        t.Dispose();
                }
            }

            return result;
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
    }
}
