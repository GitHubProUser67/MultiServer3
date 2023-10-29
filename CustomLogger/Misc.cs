using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace CustomLogger
{
    public class Misc
    {
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

        public static IPAddress? GetIp(string hostname)
        {
            try
            {

                if (hostname.ToLower() == "localhost" || hostname == "127.0.0.1")
                    return IPAddress.Loopback;
                switch (Uri.CheckHostName(hostname))
                {
                    case UriHostNameType.IPv4: return IPAddress.Parse(hostname);
                    case UriHostNameType.Dns: return Dns.GetHostAddresses(hostname).FirstOrDefault()?.MapToIPv4() ?? IPAddress.Any;
                    default:
                        {
                            return null;
                        }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetIp] - An Error Occured - {ex}");
            }

            return null;
        }

        public static string GetPublicIPAddress()
        {
            try
            {
                string IPAddress = string.Empty;
                IPHostEntry Host = default(IPHostEntry);
                string? Hostname = null;
                Hostname = Environment.MachineName;
                Host = Dns.GetHostEntry(Hostname);
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == AddressFamily.InterNetwork)
                        IPAddress = Convert.ToString(IP);
                }

                return IPAddress;
            }
            catch (Exception)
            {

            }

            return "127.0.0.1";
        }

        public static IPAddress? GetLocalIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            // Get all active interfaces
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);

            // Find our local ip
            foreach (var i in interfaces)
            {
                var props = i.GetIPProperties();
                var inter = props.UnicastAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork);
#pragma warning disable // Sometimes Visual Studio is weird.
                if (inter != null && props.GatewayAddresses.Count > 0 && inter.Count() > 0)
                    return inter.FirstOrDefault().Address;
#pragma warning restore
            }

            return null;
        }

        public static string GetFirstActiveIPAddress(string hostName, string fallback)
        {
            try
            {
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

                return fallback;
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError(ex);
                return fallback;
            }
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

                return builder.ToString();
            }
        }

        public static string ComputeSaltedSHA256(string input)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input + "MyS1lT3DPass" + ComputeMD5("ssaPD3Tl1SyM" + input)));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
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
    }
}
