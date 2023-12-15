using CustomLogger;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Security.Principal;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CryptoSporidium
{
    public class MiscUtils
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
            newArray[newArray.Length - 1] = newElement;

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
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = $"{currentDateTime:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
            return formattedDateTime;
        }

        public static string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            long ticks = DateTime.Now.Ticks;
            long nanoseconds = (ticks % TimeSpan.TicksPerMillisecond) * 100;

            return nanoseconds.ToString("00000000"); // Pad with zeros to 8 digits
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            int len = hex.Length;
            byte[] byteArray = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return byteArray;
        }

        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
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

            int totalLength = first.Length + second.Sum(arr => arr.Length);
            byte[] result = new byte[totalLength];

            Buffer.BlockCopy(first, 0, result, 0, first.Length);

            int offset = first.Length;
            foreach (var array in second)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
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

        public static bool FindbyteSequence(byte[] byteArray, byte[] sequenceToFind)
        {
            try
            {
                if (Avx2.IsSupported)
                {
                    // Compare the first element
                    Vector256<byte> compareResult = new();

                    for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                    {
                        // Compare the first element
                        compareResult = Avx2.CompareEqual(Vector256<byte>.Zero.WithElement(0, byteArray[i]), Vector256<byte>.Zero.WithElement(0, sequenceToFind[0]));

                        // Extract the result to check if the first element matches
                        if (Avx2.MoveMask(compareResult) != 0)
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
                    // Compare the first element
                    Vector128<byte> compareResult = new();

                    for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                    {
                        // Compare the first element
                        compareResult = Sse2.CompareEqual(Vector128<byte>.Zero.WithElement(0, byteArray[i]), Vector128<byte>.Zero.WithElement(0, sequenceToFind[0]));

                        // Extract the result to check if the first element matches
                        if (Sse2.MoveMask(compareResult) != 0)
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
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MiscUtils] - Server has throw an exception in FindbyteSequence : {ex}");
            }

            return false;
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

        public static IPAddress? GetIp(string hostname)
        {
            try
            {
                if (hostname.ToLower() == "localhost" || hostname == "127.0.0.1")
                    return IPAddress.Loopback;

                switch (Uri.CheckHostName(hostname))
                {
                    case UriHostNameType.IPv4:
                        return IPAddress.Parse(hostname);
                    case UriHostNameType.IPv6:
                        return IPAddress.Parse(hostname);
                    case UriHostNameType.Dns:
                        IPAddress[] addresses = Dns.GetHostAddresses(hostname);
                        foreach (IPAddress address in addresses)
                        {
                            if (address.AddressFamily == AddressFamily.InterNetworkV6 || address.AddressFamily == AddressFamily.InterNetwork)
                                return address;
                        }
                        // Fallback
                        return addresses.FirstOrDefault()?.MapToIPv4() ?? IPAddress.Any;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetIp] - An Error Occurred - {ex}");
            }

            return IPAddress.Loopback;
        }

        public static string GetPublicIPAddress(bool allowipv6 = false)
        {
            using (HttpClient client = new())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(allowipv6 ? "http://icanhazip.com/" : "http://ipv4.icanhazip.com/").Result;
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().Result.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Trim();
                }
                catch (HttpRequestException)
                {

                }
            }

            return GetLocalIPAddress().ToString();
        }

        public static IPAddress GetLocalIPAddress(bool allowIPv6 = false)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return IPAddress.Parse("127.0.0.1");

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

            }

            // If no valid interface with the desired IP version is found.
            return IPAddress.Parse("127.0.0.1");
        }

        public static string GetFirstActiveIPAddress(string hostName, string fallback)
        {
            try
            {
                // Try using Google DNS (8.8.8.8) first
                IPAddress[] googleDnsAddresses = Dns.GetHostAddresses("8.8.8.8");

                foreach (IPAddress googleDnsAddress in googleDnsAddresses)
                {
                    using (var ping = new Ping())
                    {
                        try
                        {
                            PingReply reply = ping.Send(googleDnsAddress);
                            if (reply.Status == IPStatus.Success)
                            {
                                // If successful, use the resolved IP address for the original host
                                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                                foreach (IPAddress address in addresses)
                                {
                                    using (var hostPing = new Ping())
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
                    IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                    foreach (IPAddress address in addresses)
                    {
                        using (var ping = new Ping())
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
                    IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                    IPAddress[] addresses = hostEntry.AddressList;

                    foreach (IPAddress address in addresses)
                    {
                        using (var ping = new Ping())
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
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 11001)
                    LoggerAccessor.LogError($"[MiscUtils] - GetFirstActiveIPAddress thrown a socket exception : {ex}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MiscUtils] - GetFirstActiveIPAddress thrown an exception : {ex}");
            }

            return fallback;
        }

        public static bool IsTcpPortOpen(string host, int port)
        {
            using (TcpClient tcpClient = new())
            {
                try
                {
                    tcpClient.Connect(host, port);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsUdpPortOpen(string host, int port)
        {
            using (UdpClient udpClient = new())
            {
                try
                {
                    udpClient.Connect(host, port);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsWindows()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT;
        }
#pragma warning disable
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore
        public static bool IsFileOutdated(string filePath, TimeSpan maxAge)
        {
            if (!File.Exists(filePath))
                return true; // The file is outdated

            DateTime lastModified = File.GetLastWriteTime(filePath);
            DateTime currentTime = DateTime.Now;

            if (lastModified < currentTime - maxAge)
                return true; // The file is outdated

            return false; // The file is up to date
        }

        public static string ComputeMD5(string input)
        {
            // Create a SHA256   
            using (MD5 md5Hash = MD5.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
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
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                sha256Hash.Clear();

                return builder.ToString();
            }
        }
    }
}
