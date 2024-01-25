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

namespace BackendProject.MiscUtils
{
    public static class StaticVariousUtils
    {
        public static string ChopOffBefore(this string s, string Before)
        {
            //Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(Before.ToUpper());
            if (End > -1)
                return s[(End + Before.Length)..];
            return s;
        }

        public static string ChopOffAfter(this string s, string After)
        {
            //Usefull function for chopping up strings
            int End = s.ToUpper().IndexOf(After.ToUpper());
            if (End > -1)
                return s[..End];
            return s;
        }

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

        public static object? GetValueFromJToken(JToken jToken, string propertyName)
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

            return null;
        }

        public static string GetCurrentDateTime()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
        }

        public static string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            return (DateTime.Now.Ticks % TimeSpan.TicksPerMillisecond * 100).ToString("00000000"); // Pad with zeros to 8 digits
        }

        public static uint GetUnixTimeStamp()
        {
            return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new(byteArray.Length * 2);

            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

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
        /// Convert a hex-formatted string to byte array.
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

        // Alternative but with a cleanup.
        public static byte[] HexStringToByteArrayWithCleanup(string invalue)
        {
            string cleanedRequest = invalue.Replace(" ", string.Empty).Replace("\n", string.Empty);
            return Enumerable.Range(0, cleanedRequest.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(cleanedRequest.Substring(x, 2), 16))
                                 .ToArray();
        }

        public static byte[][] SplitAt(byte[] source, int index)
        {
            byte[] first = new byte[index];
            byte[] second = new byte[source.Length - index];
            Array.Copy(source, 0, first, 0, index);
            Array.Copy(source, index, second, 0, source.Length - index);
            return new[] { first, second };
        }

        public static byte[] CombineByteArray(byte[] first, byte[]? second)
        {
            if (second == null)
                return first;

            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

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

        public static byte[] ReadSmallFileChunck(string filePath, int bytesToRead)
        {
            byte[] result = new byte[bytesToRead];

            using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader reader = new(fileStream))
                {
                    int bytesRead = reader.Read(result, 0, bytesToRead);

                    // If the file is less than 10 bytes, pad with null bytes
                    for (int i = bytesRead; i < bytesToRead; i++)
                    {
                        result[i] = 0;
                    }
                    reader.Close();
                }
            }

            return result;
        }

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

        public static byte[] XORBytes(byte[] array1, byte[] array2)
        {
            return array1.Zip(array2, (x, y) => (byte)(x ^ y)).ToArray();
        }

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
        /// Returns index of provided byte pattern in a buffer,
        /// returns -1 if not found
        /// </summary>
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
        /// Returns index of provided byte pattern in a buffer,
        /// returns -1 if not found
        /// </summary>
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

        public static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string TrimString(byte[] str)
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

        public static string ComputeMD5(string input)
        {
            // Create a MD5   
            using (MD5 md5Hash = MD5.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                md5Hash.Clear();

                return builder.ToString();
            }
        }

        public static string ComputeSHA256(string input)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                sha256Hash.Clear();

                return builder.ToString();
            }
        }

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

        public static IPAddress GetLocalIPAddress(bool allowIPv6 = false)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return IPAddress.Loopback;

                // Get all network interfaces on the machine.
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                // Filter out loopback and non-operational interfaces.
                var validInterfaces = networkInterfaces
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && !n.Description.ToLowerInvariant().Contains("virtual"));

                // Find the first valid interface with the desired IP version.
                foreach (NetworkInterface? networkInterface in validInterfaces)
                {
                    IPInterfaceProperties? properties = networkInterface.GetIPProperties();

                    // Filter out non-IPv4 or non-IPv6 addresses based on the allowIPv6 parameter.
                    var addresses = allowIPv6
                        ? properties.UnicastAddresses.Select(addr => addr.Address.ToString())
                        : properties.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            .Select(addr => addr.Address.ToString());

                    // If there is at least one address, return the first one
                    if (addresses.Any())
                        return IPAddress.Parse(addresses.First());
                }
            }
            catch (Exception)
            {
                // Not Important.
            }

            // If no valid interface with the desired IP version is found.
            return IPAddress.Loopback;
        }

        public static string GetFirstActiveIPAddress(string hostName, string fallback)
        {
            try
            {
                // Try using Google DNS (8.8.8.8) first
                foreach (IPAddress googleDnsAddress in Dns.GetHostAddresses("8.8.8.8"))
                {
                    using (Ping ping = new())
                    {
                        try
                        {
                            if (ping.Send(googleDnsAddress).Status == IPStatus.Success)
                            {
                                // If successful, use the resolved IP address for the original host
                                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                                foreach (IPAddress address in addresses)
                                {
                                    using (Ping hostPing = new())
                                    {
                                        try
                                        {
                                            PingReply hostReply = hostPing.Send(address);
                                            if (hostReply.Status == IPStatus.Success)
                                                return address.ToString();
                                        }
                                        catch (PingException)
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        catch (PingException)
                        {
                            continue;
                        }
                    }
                }

                // If Google DNS resolution fails, fall back to the existing logic
                if (IsWindows())
                {
                    foreach (IPAddress address in Dns.GetHostAddresses(hostName))
                    {
                        using (Ping ping = new())
                        {
                            try
                            {
                                PingReply reply = ping.Send(address);
                                if (reply.Status == IPStatus.Success)
                                    return address.ToString();
                            }
                            catch (PingException)
                            {
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    foreach (IPAddress address in Dns.GetHostEntry(hostName).AddressList)
                    {
                        using (Ping ping = new())
                        {
                            try
                            {
                                PingReply reply = ping.Send(address);
                                if (reply.Status == IPStatus.Success)
                                    return address.ToString();
                            }
                            catch (PingException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Not Important.
            }

            return fallback;
        }

        public static bool IsTCPPortAvailable(int port)
        {
            try
            {
                new TcpClient().Connect(IPAddress.Loopback, port);
            }
            catch (Exception)
            {
                // The port is available as connection failed.
                return true;
            }

            // The port is in use as we could connect to it.
            return false;
        }

        public static bool IsUDPPortAvailable(int port)
        {
            try
            {
                using (UdpClient udpClient = new(port))
                {
                    udpClient.Close();
                }
            }
            catch (Exception)
            {
                // If an exception occurs, the port is already in use.
                return false;
            }

            // If everything goes fine, means the port is free.
            return true;
        }

        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
        }
#pragma warning disable
        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore
        public static bool IsFileOutdated(string filePath, TimeSpan maxAge)
        {
            if (!File.Exists(filePath))
                return true; // The file is outdated

            if (File.GetLastWriteTime(filePath) < DateTime.Now - maxAge)
                return true; // The file is outdated

            return false; // The file is up to date
        }

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
                default:
                    pstring = "OTHER";
                    break;
            }
            return $"{pstring}/1.0 UPnP/1.0 DLNADOC/1.5 sdlna/1.0";
        }
    }
}
