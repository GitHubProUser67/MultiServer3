using CustomLogger;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Security.Principal;

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

        public string? ExtractFirst16Characters(string input)
        {
            // Check if the input string is not null and has at least 16 characters
            if (input != null && input.Length >= 16)
                // Use Substring to get the first 16 characters
                return input.Substring(0, 16);
            return null;
        }

        public static string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            long ticks = DateTime.Now.Ticks;
            long nanoseconds = (ticks % TimeSpan.TicksPerMillisecond) * 100;

            return nanoseconds.ToString("00000000"); // Pad with zeros to 8 digits
        }

        public static string GetCurrentDateTime()
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = $"{currentDateTime:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
            return formattedDateTime;
        }

        public static string GenerateDynamicCacheGuid(string input)
        {
            string md5hash = string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(GetCurrentDateTime() + input));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Clear();
            }

            return md5hash;
        }

        public byte[] ReadBinaryFile(string filePath, int offset, int length)
        {
            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                byte[] data = new byte[length];
                fs.Read(data, 0, length);

                fs.Flush();

                return data;
            }
        }

        public int FindDataPosInBinary(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length - data2.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < data2.Length; j++)
                {
                    if (data1[i + j] != data2[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return i;
            }

            return -1; // Data2 not found in Data1
        }

        public byte[] HexStringToByteArray(string hex)
        {
            int len = hex.Length;
            byte[] byteArray = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return byteArray;
        }

        public string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        public string StringToHexString(string input, string formatExpression)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            StringBuilder hexBuilder = new(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hexBuilder.AppendFormat(formatExpression, b);
            }

            return hexBuilder.ToString();
        }

        public byte[] ConcatenateArrays(byte[][] arrays)
        {
            int totalLength = arrays.Sum(arr => arr.Length);
            byte[] result = new byte[totalLength];
            int offset = 0;

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        public byte[]? GetRequiredBlocks(byte[] byteArray, int blockSize)
        {
            if (blockSize <= 0)
            {
                LoggerAccessor.LogError("Block size must be greater than zero.");
                return null;
            }

            if (byteArray.Length == 0)
                return new byte[0]; // If the input array is empty, return an empty array.

            // Create a new byte array with the calculated length.
            byte[] result = new byte[blockSize];

            // Copy the required blocks from the input array to the result array.
            Array.Copy(byteArray, result, blockSize);

            return result;
        }

        public byte[] ReverseByteArray(byte[] input)
        {
            byte[] reversedArray = new byte[input.Length];
            int lastIndex = input.Length - 1;

            for (int i = 0; i < input.Length; i++)
            {
                reversedArray[i] = input[lastIndex - i];
            }

            return reversedArray;
        }

        public byte[] Combinebytearay(byte[] first, byte[]? second)
        {
            if (second == null)
                return first;

            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public bool FindbyteSequence(byte[] byteArray, byte[] sequenceToFind)
        {
            try
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

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server has throw an exception in FindRPCNSequence : {ex}");

                return false;
            }
        }

        public byte[]? CopyBytes(byte[] source, uint size)
        {
            if (source == null)
                return null;

            if (size > source.Length)
            {
                LoggerAccessor.LogError($"Size exceeds the length of the source array | SRC -> {source.Length} | DST -> {size}.");
                return null;
            }

            byte[] result = new byte[size];
            Array.Copy(source, result, (int)size);
            return result;
        }

        public byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }

        public byte[] TrimStart(byte[] byteArray, int index)
        {
            if (index >= byteArray.Length)
            {
                // If the index is greater than or equal to the length of the array,
                // return an empty byte array.
                return new byte[0];
            }
            else
            {
                // Create a new byte array starting from the specified index.
                byte[] trimmedArray = new byte[byteArray.Length - index];
                Array.Copy(byteArray, index, trimmedArray, 0, trimmedArray.Length);
                return trimmedArray;
            }
        }

        public byte[]? TrimBytes(byte[] source, uint size)
        {
            if (source == null)
                return null;

            if (size >= source.Length)
                return new byte[0]; // Return an empty array if size is greater than or equal to the source length.

            byte[] result = new byte[source.Length - size];
            Array.Copy(source, size, result, 0, result.Length);
            return result;
        }

        public string TrimString(byte[] str)
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

        public byte[]? ConvertSha1StringToByteArray(string sha1String)
        {
            if (sha1String.Length % 2 != 0)
            {
                LoggerAccessor.LogError("Input string length must be even.");
                return null;
            }

            byte[] byteArray = new byte[sha1String.Length / 2];

            for (int i = 0; i < sha1String.Length; i += 2)
            {
                string hexByte = sha1String.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return byteArray;
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
                            if (address.AddressFamily == AddressFamily.InterNetworkV6)
                                return address;
                        }
                        return addresses.FirstOrDefault()?.MapToIPv4() ?? IPAddress.Any;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetIp] - An Error Occurred - {ex}");
            }

            return null;
        }

        public static string GetPublicIPAddress(bool allowipv6 = false)
        {
            try
            {
                if (allowipv6)
                    return new WebClient().DownloadString("http://icanhazip.com/").Replace("\\r\\n", "").Replace("\\n", "").Trim();
                else
                    return new WebClient().DownloadString("http://ipv4.icanhazip.com/").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            }
            catch (Exception)
            {

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

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool StartAsAdmin(string filePath)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo =
                    {
                        FileName = filePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };

                proc.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string XORString(string input, string key)
        {
            StringBuilder result = new();

            for (int i = 0; i < input.Length; i++)
            {
                result.Append((char)(input[i] ^ key[i % key.Length]));
            }

            return result.ToString();
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

        public static string ComputeSHA512ReducedSizeCustom(string input)
        {
            // Create a SHA256 that is calculated 2 times.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] PassBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(new MiscUtils().ByteArrayToHexString(sha256Hash.ComputeHash
                    (Encoding.UTF8.GetBytes(XORString(input + "ssaPD3Tl1SyM" + ComputeMD5("MyS1lT3DPass" + input), ReverseString(input)))))));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < PassBytes.Length; i++)
                    builder.Append(PassBytes[i].ToString("x2"));

                sha256Hash.Clear();

                return builder.ToString().ToUpper(); // To Upper for a nicer output.
            }
        }

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
    }
}
