using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkLibrary.Extension
{
    public static class StringUtils
    {
        public static string ChopOffBefore(this string s, string Before)
        {
            // Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(Before.ToUpper());
            if (End > -1)
                return s.Substring(End + Before.Length);

            return s;
        }

        public static string ChopOffAfter(this string s, string After)
        {
            // Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(After.ToUpper());
            if (End > -1)
                return s.Substring(0, End);
            return s;
        }

        public static string ReplaceIgnoreCase(this string Source, string Pattern, string Replacement)
        {
            // using \\$ in the pattern will screw this regex up
            // return Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

            if (Regex.IsMatch(Source, Pattern, RegexOptions.IgnoreCase))
                Source = Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

            return Source;
        }

        public static string TrimBeforeExtension(this string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName).Trim() + Path.GetExtension(fileName);
        }

        /// <summary>
        /// Transform a string to it's hexadecimal representation.
        /// <para>Obtenir un string dans sa représentation hexadecimale.</para>
        /// <param name="str">The string to transform.</param>
        /// </summary>
        /// <returns>A string.</returns>
        public static string ToHexString(this string str)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte t in Encoding.UTF8.GetBytes(str))
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string HexStringToString(this string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Convert a hex-formatted string to byte array.
        /// <para>Convertir une représentation hexadécimal en tableau de bytes.</para>
        /// </summary>
        /// <param name="hex">A string looking like "300D06092A864886F70D0101050500".</param>
        /// <returns>A byte array.</returns>
        public static byte[] HexStringToByteArray(this string hex)
        {
            string cleanedRequest = hex.Replace(" ", string.Empty).Replace("\n", string.Empty);

            if (cleanedRequest.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[cleanedRequest.Length >> 1];

            for (int i = 0; i < cleanedRequest.Length >> 1; ++i)
            {
                arr[i] = (byte)((CharUtils.GetHexVal(cleanedRequest[i << 1]) << 4) + CharUtils.GetHexVal(cleanedRequest[(i << 1) + 1]));
            }

            return arr;
        }

        /// <summary>
        /// Verify is the string is in base64 format.
        /// <para>Vérifie si un string est en format base64.</para>
        /// </summary>
        /// <param name="base64String">The base64 string.</param>
        /// <returns>A tuple boolean, byte array.</returns>
        public static (bool, byte[]) IsBase64(this string base64String)
        {
            // Credit: oybek https://stackoverflow.com/users/794764/oybek
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t")
               || base64String.Contains("\r") || base64String.Contains("\n"))
                return (false, null);

#if NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = new byte[((base64String.Length * 3) + 3) / 4 -
                (base64String.Length > 0 && base64String[base64String.Length - 1] == '=' ?
                    base64String.Length > 1 && base64String[base64String.Length - 2] == '=' ?
                        2 : 1 : 0)];

            if (Convert.TryFromBase64String(base64String, buffer, out int bytesWritten) && bytesWritten > 0)
                return (true, buffer.ToArray());
#else
            try
            {
                byte[] buffer = Convert.FromBase64String(base64String);

                if (buffer.Length > 0)
                    return (true, buffer);
            }
            catch
            {

            }
#endif

            return (false, null);
        }

        public static Stream ToStream(this string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Adds an element to a double string array.
        /// <para>Ajoute un élément à une liste double de strings.</para>
        /// </summary>
        /// <param name="original">The original double array.</param>
        /// <param name="bytesToRead">The new array to add.</param>
        /// <returns>A double array of strings.</returns>
        public static string[][] AddArray(this string[][] original, string[] newElement)
        {
            int newSize = original.Length + 1;
            string[][] newArray = new string[newSize][];
            Parallel.For(0, original.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (i, stateInner) =>
            {
                newArray[i] = original[i];
            });
            newArray[newSize - 1] = newElement;
            return newArray;
        }
    }
}
