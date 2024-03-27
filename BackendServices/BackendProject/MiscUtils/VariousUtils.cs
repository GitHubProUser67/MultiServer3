using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Security.Principal;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using ArpLookup;

namespace BackendProject.MiscUtils
{
    public static class StaticVariousUtils
    {
        /// <summary>
        /// Chop a string before a given text.
        /// <para>Coupe un string avant un caractère spécial.</para>
        /// </summary>
        /// <param name="s">A classic string.</param>
        /// <param name="Before">the delimiting string.</param>
        /// <returns>A string.</returns>
        public static string ChopOffBefore(this string s, string Before)
        {
            //Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(Before.ToUpper());
            if (End > -1)
                return s[(End + Before.Length)..];
            return s;
        }

        /// <summary>
        /// Chop a string after a given text.
        /// <para>Coupe un string après un caractère spécial.</para>
        /// </summary>
        /// <param name="s">A classic string.</param>
        /// <param name="After">the delimiting string.</param>
        /// <returns>A string.</returns>
        public static string ChopOffAfter(this string s, string After)
        {
            //Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(After.ToUpper());
            if (End > -1)
                return s[..End];
            return s;
        }

        /// <summary>
        /// Apply regex to a string.
        /// <para>Applique une formule regex sur un string.</para>
        /// </summary>
        /// <param name="Source">A classic string.</param>
        /// <param name="Pattern">the pathern.</param>
        /// <param name="Replacement">the replacement string.</param>
        /// <returns>A string.</returns>
        public static string ReplaceIgnoreCase(this string Source, string Pattern, string Replacement)
        {
            // using \\$ in the pattern will screw this regex up
            //return Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

            if (Regex.IsMatch(Source, Pattern, RegexOptions.IgnoreCase))
                Source = Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);
            return Source;
        }
    }

    public class VariousUtils
    {
        /// <summary>
        /// Add a dynamic array to a existing array structure.
        /// <para>Ajoute un objet dynamique sur une structure existante.</para>
        /// </summary>
        /// <param name="jaggedArray">A complex array.</param>
        /// <param name="newElement">the complex element to add.</param>
        /// <returns>A complex jagguedArray.</returns>
        public static T[][] AddElementToLastPosition<T>(T[][] jaggedArray, T[] newElement)
        {
            // Create a new jagged array with increased size
            T[][] newArray = new T[jaggedArray.Length + 1][];

            // Copy existing elements to the new array
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                newArray[i] = jaggedArray[i];
            }

            // Add the new element to the last position
            newArray[^1] = newElement;

            return newArray;
        }

        /// <summary>
        /// Add multiple dynamic arrays to an existing array structure.
        /// <para>Ajoute plusieurs objets dynamiques sur une structure existante.</para>
        /// </summary>
        /// <typeparam name="T">Type of the elements in the arrays.</typeparam>
        /// <param name="jaggedArray">A complex array.</param>
        /// <param name="newElements">The complex elements to add.</param>
        /// <returns>A complex jagged array.</returns>
        public static T[][] AddElementsToLastPosition<T>(T[][] jaggedArray, params T[][] newElements)
        {
            // Create a new jagged array with increased size
            T[][] newArray = new T[jaggedArray.Length + newElements.Length][];

            // Copy existing elements to the new array
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                newArray[i] = jaggedArray[i];
            }

            // Add the new elements to the last positions
            for (int i = 0; i < newElements.Length; i++)
            {
                newArray[jaggedArray.Length + i] = newElements[i];
            }

            return newArray;
        }

        /// <summary>
        /// Parse a newtonsoft json JToken.
        /// <para>Parcours un Jtoken issue de la librairie newtonsoft json.</para>
        /// </summary>
        /// <param name="jToken">A newtonsoft json jtoken.</param>
        /// <param name="propertyName">the property to parse in the jtoken.</param>
        /// <returns>A complex object.</returns>
        public static object? GetValueFromJToken(JToken jToken, string propertyName)
        {
            try
            {
                JToken? valueToken = jToken[propertyName];

                if (valueToken != null)
                {
                    if (valueToken.Type == JTokenType.Object || valueToken.Type == JTokenType.Array)
                        return valueToken.ToObject<object>();
                    else if (valueToken.Type == JTokenType.Integer)
                        return valueToken.ToObject<int>();
                    else if (valueToken.Type == JTokenType.String)
                        return valueToken.ToObject<string>();
                    else if (valueToken.Type == JTokenType.Boolean)
                        return valueToken.ToObject<bool>();
                    else if (valueToken.Type == JTokenType.Float)
                        return valueToken.ToObject<float>();
                }
            }
            catch (Exception)
            {
                // Not Important.
            }

            return null;
        }

        public static Dictionary<object, object> ExtractKeyValues(string jsonString, string nameProperty)
        {
            var jsonObject = JObject.Parse(jsonString);
            var result = new Dictionary<object, object>();

            if (jsonObject.TryGetValue(nameProperty, out var dataToken) && dataToken is JObject dataObject)
            {
                foreach (var property in dataObject.Properties())
                {
                    result.Add(property.Name, property.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the current date-time.
        /// <para>Obtenir la date actuelle.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static string GetCurrentDateTime()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
        }

        /// <summary>
        /// Get Nanoseconds of the current date-time.
        /// <para>Obtenir la date actuelle avec une évaluation en nano-secondes.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            return (DateTime.Now.Ticks % TimeSpan.TicksPerMillisecond * 100).ToString("00000000"); // Pad with zeros to 8 digits
        }

        /// <summary>
        /// Get the current date-time in unix format.
        /// <para>Obtenir la date actuelle en format unix.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static uint GetUnixTimeStamp()
        {
            return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Transform a byte array to it's hexadecimal representation.
        /// <para>Obtenir un tableau de bytes dans sa représentation hexadecimale.</para>
        /// <param name="byteArray">The byte array to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new(byteArray.Length * 2);

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
            if (arr1.Length != arr2.Length)
                return false;

            // Compare each element in the arrays
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }

            // If all elements are identical, return true
            return true;
        }

        /// <summary>
        /// Check if 2 strings lists are strictly identical.
        /// <para>Savoir si 2 listes de strings sont strictement identiques.</para>
        /// <param name="list1">The left list.</param>
        /// <param name="list2">The right list.</param>
        /// </summary>
        /// <returns>A boolean.</returns>
        public static bool AreListsIdentical(List<string> list1, List<string> list2)
        {
            // If lists have different counts, they are not identical
            if (list1.Count != list2.Count)
                return false;

            // Sort the lists to ensure elements are in the same order for comparison
            list1.Sort();
            list2.Sort();

            // Compare each element in the lists

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            // Lists are identical
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
            Span<byte> buffer = new(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
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
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Convert a hex-formatted string to byte array with an extra cleanup.
        /// <para>Convertir une représentation hexadécimal en tableau de bytes avec un netoyage supplémentaire.</para>
        /// </summary>
        /// <param name="hex">A string looking like "300D06092A864886F70D0101050500".</param>
        /// <returns>A byte array.</returns>
        public static byte[] HexStringToByteArrayWithCleanup(string hex)
        {
            string cleanedRequest = hex.Replace(" ", string.Empty).Replace("\n", string.Empty);
            return Enumerable.Range(0, cleanedRequest.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(cleanedRequest.Substring(x, 2), 16))
                                 .ToArray();
        }

        /// <summary>
        /// Split a byte array at a given index.
        /// <para>Diviser un tableau de bytes sur un index donné.</para>
        /// </summary>
        /// <param name="source">The byte array to divide in 2 parts.</param>
        /// <param name="index">The uint index at which we must separate.</param>
        /// <returns>An array of byte array.</returns>
        public static byte[][] SplitAt(byte[] source, int index)
        {
            byte[] first = new byte[index];
            byte[] second = new byte[source.Length - index];
            Array.Copy(source, 0, first, 0, index);
            Array.Copy(source, index, second, 0, source.Length - index);
            return new[] { first, second };
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
        /// Copies a given amount of bytes within a byte array with a given offset and length.
        /// <para>Copier un montant déterminé de bytes contenus dans un tableau de bytes avec comme indicateur, la position ainsi que la taille souhaité.</para>
        /// </summary>
        /// <param name="source">The first byte array at which we want to copy stuff from.</param>
        /// <param name="offset">The offset at which we must start to copy process.</param>
        /// <param name="offset">The length we want to copy.</param>
        /// <returns>A byte array.</returns>
        public static byte[]? CopyBytes(byte[] source, int offset, int length)
        {
            if (source == null || offset < 0 || length < 0 || offset >= source.Length)
                return null;

            if (source.Length > length)
            {
                byte[] result = new byte[length];
                Buffer.BlockCopy(source, offset, result, 0, result.Length);
                return result;
            }
            else
            {
                byte[] result = new byte[source.Length];
                Buffer.BlockCopy(source, offset, result, 0, result.Length);
                return result;
            }
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
            byte[] result = new byte[bytesToRead];

            using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using BinaryReader reader = new(fileStream);
                int bytesRead = reader.Read(result, 0, bytesToRead);

                // If the file is less than 10 bytes, pad with null bytes
                for (int i = bytesRead; i < bytesToRead; i++)
                {
                    result[i] = 0;
                }
                reader.Close();
            }

            return result;
        }

        /// <summary>
        /// Assemble an array of byte array to a single byte array.
        /// <para>Assemble un tableau de tableaux de bytes en un unique tableau de bytes.</para>
        /// </summary>
        /// <param name="arrays">The input array of byte array.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ConcatenateArrays(byte[][] arrays)
        {
            byte[] result = new byte[arrays.Sum(arr => arr.Length)];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        /// <summary>
        /// Reverse a byte array (When c# fails in some very rare occurances).
        /// <para>Retourne un tableau de bytes (Quand c# n'arrive pas à le faire à de rare moments).</para>
        /// </summary>
        /// <param name="input">The input byte array.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReverseByteArray(byte[] input)
        {
            byte[] reversedArray = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                reversedArray[i] = input[input.Length - 1 - i];
            }

            return reversedArray;
        }

        /// <summary>
        /// Apply a XOR mathematical calculation between 2 byte arrays.
        /// <para>Applique un calcul mathématique de type OU exclusif entre 2 tableaux de bytes.</para>
        /// </summary>
        /// <param name="array1">The left array.</param>
        /// <param name="array2">The right array.</param>
        /// <returns>A byte array.</returns>
        public static byte[] XORBytes(byte[] array1, byte[] array2)
        {
            return array1.Zip(array2, (x, y) => (byte)(x ^ y)).ToArray();
        }

        /// <summary>
        /// Finds a sequence of bytes within a byte array.
        /// <para>Trouve une séquence de bytes dans un tableau de bytes.</para>
        /// </summary>
        /// <param name="byteArray">The array in which we search for the sequence.</param>
        /// <param name="sequenceToFind">The byte array sequence to find.</param>
        /// <returns>A boolean.</returns>
        public static bool FindbyteSequence(byte[] byteArray, byte[] sequenceToFind)
        {
            if (Avx2.IsSupported)
            {
                for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                {
                    // Extract the result to check if the first element matches
                    if (Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, byteArray[i]), Vector256<byte>.Zero.WithElement(0, sequenceToFind[0]))) != 0)
                    {
                        // Check the remaining elements
                        bool found = true;
                        for (int j = 1; j < sequenceToFind.Length; j++)
                        {
                            if (byteArray[i + j] != sequenceToFind[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                            return true;
                    }
                }
            }
            else if (Sse2.IsSupported)
            {
                for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                {
                    // Extract the result to check if the first element matches
                    if (Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, byteArray[i]), Vector128<byte>.Zero.WithElement(0, sequenceToFind[0]))) != 0)
                    {
                        // Check the remaining elements
                        bool found = true;
                        for (int j = 1; j < sequenceToFind.Length; j++)
                        {
                            if (byteArray[i + j] != sequenceToFind[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                            return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                {
                    if (byteArray[i] == sequenceToFind[0])
                    {
                        bool found = true;
                        for (int j = 1; j < sequenceToFind.Length; j++)
                        {
                            if (byteArray[i + j] != sequenceToFind[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                            return true;
                    }
                }
            }

            return false;
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
                    if (Avx2.IsSupported)
                    {
                        // Extract the result to check if the first element matches
                        if (Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, buffer[i]), Vector256<byte>.Zero.WithElement(0, searchPattern[0]))) != 0)
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
                    else if (Sse2.IsSupported)
                    {
                        // Extract the result to check if the first element matches
                        if (Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, buffer[i]), Vector128<byte>.Zero.WithElement(0, searchPattern[0]))) != 0)
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
                if (Avx2.IsSupported)
                {
                    // Extract the result to check if the first element matches
                    if (Avx2.MoveMask(Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, buffer[i]), Vector256<byte>.Zero.WithElement(0, searchPattern[0]))) != 0 && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                        return i;
                }
                else if (Sse2.IsSupported)
                {
                    // Extract the result to check if the first element matches
                    if (Sse2.MoveMask(Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, buffer[i]), Vector128<byte>.Zero.WithElement(0, searchPattern[0]))) != 0 && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                        return i;
                }
                else if (buffer[i] == searchPattern[0] && buffer.Slice(i, searchPattern.Length).SequenceEqual(searchPattern))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Adds a prefix to the file extension.
        /// <para>Ajoute un préfixe à l'extension du fichier.</para>
        /// </summary>
        /// <param name="filePath">The input filePath.</param>
        /// <param name="insertion">The string to insert before file ext.</param>
        /// <returns>A string.</returns>
        public static string InsertStringBeforeExtension(string filePath, string insertion)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
                return Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(filePath)}{insertion}{Path.GetExtension(filePath)}");
            else
                return filePath;
        }

        /// <summary>
        /// Converts a string to it's hexadecimal respresentation.
        /// <para>Transforme un string en sa représatation en hexadécimal.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string.</returns>
        public static string StringToHexString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder hexStringBuilder = new(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hexStringBuilder.AppendFormat("{0:x2}", b);
            }

            return hexStringBuilder.ToString();
        }

        /// <summary>
        /// Reverse the characters of a string.
        /// <para>Renverse les caractère d'un string.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string.</returns>
        public static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Trim a byte array to a string result.
        /// <para>Coupe un tableau de bytes et convertis le résultat en string.</para>
        /// </summary>
        /// <param name="str">The input byte array to trim.</param>
        /// <returns>A string.</returns>
        public static string TrimByteArraytoString(byte[] str)
        {
            int i = str.Length - 1;
            while (str[i] == 0)
            {
                Array.Resize(ref str, i);
                i -= 1;
            }
            string res = Encoding.ASCII.GetString(str);
            //if (res.ToLower() == "www") return null; Some sites do not work without www
            /* else*/
            return res;
        }

        /// <summary>
        /// Extract a portion of a string winthin boundaries.
        /// <para>Extrait une portion d'un string entre des limites.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="startToRemove">The amount of characters to remove from the left to the right.</param>
        /// <param name="endToRemove">The amount of characters to remove from the right to the left.</param>
        /// <returns>A string.</returns>
        public static string? ExtractPortion(string input, int startToRemove, int endToRemove)
        {
            if (input.Length < startToRemove + endToRemove)
                return null;

            return input[startToRemove..][..^endToRemove];
        }

        /// <summary>
        /// Compute the MD5 checksum of a stream.
        /// <para>Calcul la somme des contrôles en MD5 d'un stream.</para>
        /// </summary>
        /// <param name="input">The input stream (must be seekable).</param>
        /// <returns>A string.</returns>
        public static string ComputeMD5(Stream input)
        {
            // ComputeHash - returns byte array  
            byte[] bytes = MD5.Create().ComputeHash(input);

            input.Position = 0;

            // Convert byte array to a string   
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Compute the MD5 checksum of a byte array.
        /// <para>Calcul la somme des contrôles en MD5 d'un tableau de bytes.</para>
        /// </summary>
        /// <param name="input">The input byte array.</param>
        /// <returns>A string.</returns>
        public static string ComputeMD5(byte[] input)
        {
            // ComputeHash - returns byte array  
            byte[] bytes = MD5.Create().ComputeHash(input);

            // Convert byte array to a string   
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Compute the MD5 checksum of a string.
        /// <para>Calcul la somme des contrôles en MD5 d'un string.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string.</returns>
        public static string ComputeMD5(string input)
        {
            // ComputeHash - returns byte array  
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert byte array to a string   
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Compute the SHA256 checksum of a string.
        /// <para>Calcul la somme des contrôles en SHA256 d'un string.</para>
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string.</returns>
        public static string ComputeSHA256(string input)
        {
            // ComputeHash - returns byte array  
            byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert byte array to a string   
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        /// <summary>
        /// Get the public IP of the server.
        /// <para>Obtiens l'IP publique du server.</para>
        /// </summary>
        /// <param name="allowipv6">Allow IPV6 format.</param>
        /// <param name="ipv6urlformat">Format the IPV6 result in a url compatible format ([addr]).</param>
        /// <returns>A string.</returns>
        public static string GetPublicIPAddress(bool allowipv6 = false, bool ipv6urlformat = false)
        {
#if NET7_0
            try
            {
                HttpResponseMessage response = new HttpClient().GetAsync(allowipv6 ? "http://icanhazip.com/" : "http://ipv4.icanhazip.com/").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                if (ipv6urlformat && allowipv6 && result.Length > 15)
                    return $"[{result}]";
                else
                    return result;
            }
            catch (Exception)
            {
                // Not Important.
            }
#else
            try
            {
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                string result = new WebClient().DownloadStringTaskAsync(allowipv6 ? "http://icanhazip.com/" : "http://ipv4.icanhazip.com/").Result
#pragma warning restore
                    .Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                if (ipv6urlformat && allowipv6 && result.Length > 15)
                    return $"[{result}]";
                else
                    return result;
            }
            catch (Exception)
            {
                // Not Important.
            }
#endif

            return GetLocalIPAddress().ToString();
        }

        /// <summary>
        /// Get the local IP of the server.
        /// <para>Obtiens l'IP locale du server.</para>
        /// </summary>
        /// <param name="allowipv6">Allow IPV6 format.</param>
        /// <returns>A IPAddress.</returns>
        public static IPAddress GetLocalIPAddress(bool allowipv6 = false)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    // Find the first valid interface with the desired IP version.
                    foreach (NetworkInterface? networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus == OperationalStatus.Up && !n.Description.ToLowerInvariant().Contains("virtual")))
                    {
                        IPInterfaceProperties? properties = networkInterface.GetIPProperties();

                        // Filter out non-IPv4 or non-IPv6 addresses based on the allowIPv6 parameter.
                        var addresses = allowipv6
                            ? properties.UnicastAddresses.Select(addr => addr.Address.ToString())
                            : properties.UnicastAddresses
                                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                                .Select(addr => addr.Address.ToString());

                        // If there is at least one address, return the first one
                        if (addresses.Any())
                            return IPAddress.Parse(addresses.First());
                    }
                }
            }
            catch (Exception)
            {
                // Not Important.
            }

            // If no valid interface with the desired IP version is found.
            return IPAddress.Loopback;
        }

        /// <summary>
        /// Get the first active IP of a given domain.
        /// <para>Obtiens la première IP active disponible d'un domaine.</para>
        /// </summary>
        /// <param name="hostName">The domain on which we search.</param>
        /// <param name="fallback">The fallback IP if we fail to find any results</param>
        /// <param name="RequirePing">If we want to check if domain respond to a Ping</param>
        /// <returns>A string.</returns>
        public static string? GetFirstActiveIPAddress(string hostName, string? fallback, bool RequirePing = false)
        {
            try
            {
                if (RequirePing)
                {
                    foreach (IPAddress address in Dns.GetHostEntry(hostName).AddressList)
                    {
                        try
                        {
                            if (new Ping().Send(address).Status == IPStatus.Success)
                                return address.ToString();
                        }
                        catch (PingException)
                        {
                            continue;
                        }
                    }
                }
                else
                    return Dns.GetHostEntry(hostName).AddressList.FirstOrDefault()?.ToString() ?? fallback;
            }
            catch (Exception)
            {
                // Not Important.
            }

            return fallback;
        }

        /// <summary>
        /// Know if the given TCP port is available.
        /// <para>Savoir si le port TCP en question est disponible.</para>
        /// </summary>
        /// <param name="port">The port on which we scan.</param>
        /// <param name="ip">The optional ip on which we scan.</param>
        /// <returns>A boolean.</returns>
        public static bool IsTCPPortAvailable(int port, string ip = "localhost")
        {
            try
            {
                new TcpClient(ip, port).Close();
            }
            catch (Exception)
            {
                // The port is available as connection failed.
                return true;
            }

            // The port is in use as we could connect to it.
            return false;
        }

        /// <summary>
        /// Know if the given UDP port is available.
        /// <para>Savoir si le port UDP en question est disponible.</para>
        /// </summary>
        /// <param name="port">The port on which we scan.</param>
        /// <param name="ip">The optional ip on which we scan.</param>
        /// <returns>A boolean.</returns>
        public static bool IsUDPPortAvailable(int port, string ip = "localhost")
        {
            try
            {
                new UdpClient(ip, port).Close();
            }
            catch (Exception)
            {
                // If an exception occurs, the port is already in use.
                return false;
            }

            // If everything goes fine, means the port is free.
            return true;
        }

        /// <summary>
        /// Know if we are on the Windows operating system.
        /// <para>Savoir si on se situe sur un système d'exploitation Windows.</para>
        /// </summary>
        /// <returns>A boolean.</returns>
        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
        }

        /// <summary>
        /// Know if we are the true administrator of the Windows system.
        /// <para>Savoir si est réellement l'administrateur Windows.</para>
        /// </summary>
        /// <returns>A boolean.</returns>
#pragma warning disable
        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore

        /// <summary>
        /// Know if the file is outdated with a given maximum age.
        /// <para>Savoir si le fichier visé est obsolète grâce à un paramètre d'obsolescence.</para>
        /// </summary>
        /// <param name="filePath">The path of the file to check for.</param>
        /// <param name="maxAge">The maximum age of the file to determine it's status</param>
        /// <returns>A boolean.</returns>
        public static bool IsFileOutdated(string filePath, TimeSpan maxAge)
        {
            if (!File.Exists(filePath))
                return true; // The file is non-existant

            if (File.GetLastWriteTime(filePath) < DateTime.Now - maxAge)
                return true; // The file is outdated

            return false; // The file is up to date
        }

        /// <summary>
        /// Know if the directory is outdated with a given maximum age.
        /// <para>Savoir si le dossier visé est obsolète grâce à un paramètre d'obsolescence.</para>
        /// </summary>
        /// <param name="directoryPath">The path of the directory to check for.</param>
        /// <param name="maxAge">The maximum age of the directory to determine it's status</param>
        /// <returns>A boolean.</returns>
        public static bool IsDirectoryOutdated(string directoryPath, TimeSpan maxAge)
        {
            if (!File.Exists(directoryPath))
                return true; // The directory is non-existant

            if (Directory.GetCreationTime(directoryPath) < DateTime.Now - maxAge)
                return true; // The file is outdated

            return false; // The file is up to date
        }

        /// <summary>
        /// Generate a server signature for HTTP backends purposes.
        /// <para>Générer une signature à usage des sous-systèmes HTTP.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static string GenerateServerSignature()
        {
            string pstring = string.Empty;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    pstring = "WIN32";
                    break;
                case PlatformID.WinCE:
                    pstring = "WINCE";
                    break;
                case PlatformID.Unix:
                    pstring = "UNIX";
                    break;
                case PlatformID.Xbox:
                    pstring = "XBOX";
                    break;
                case PlatformID.MacOSX:
                    pstring = "MACOSX";
                    break;
                case PlatformID.Other:
                    pstring = "OTHER";
                    break;
                default:
                    pstring = "OTHER";
                    break;
            }
            return $"{pstring}/1.0 UPnP/1.0 DLNADOC/1.5 sdlna/1.0";
        }

        public static string GetCPUArchitecture()
        {
            string? processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (!string.IsNullOrEmpty(processorArchitecture))
                return processorArchitecture + ((processorArchitecture == "AMD64") ? string.Empty : (Environment.Is64BitProcess ? "_64" : "_32"));

            // Unsupported architecture or unable to determine
            return "Unknown";
        }

        /// <summary>
        /// Get the MAC of the Netowrk IP.
        /// </summary>
        /// <param name="ipAddress">IP to check.</param>
        /// <returns></returns>
        public static PhysicalAddress? GetMAC(IPAddress ipAddress)
        {
            if (Arp.IsSupported)
                return Arp.Lookup(ipAddress);
            else
                return null;
        }
    }
}
