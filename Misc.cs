using NLua;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace MultiServer
{
    public class Misc
    {
        /// <summary>
		/// Convert string to DateTimeOffset
		/// </summary>
		/// <param name="input">Input string</param>
		/// <returns>DateTimeOffset</returns>
		/// <exception cref="InvalidCastException">Throws if the <paramref name="input"/> is not understood by .NET Runtime.</exception>
		public static DateTimeOffset ToDateTimeOffset(string input)
        {
            //see for docs: https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.parse?view=net-6.0
            return (input.ToLower() == "now" ? DateTimeOffset.Now : DateTimeOffset.Parse(input));
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

        public static string generatedynamiccacheguid(string input)
        {
            string md5hash = "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(GetCurrentDateTime() + input));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            return md5hash;
        }

        /// <summary>
        /// From https://www.c-sharpcorner.com/blogs/how-to-get-public-ip-address-using-c-sharp1
        /// </summary>
        /// <returns></returns>
        public static string GetPublicIPAddress()
        {
            string address;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        public static bool IsHTTPServerAccessible(string ip)
        {
            try
            {
                WebRequest request = WebRequest.Create($"http://{ip}/");
                using (WebResponse response = request.GetResponse())
                {
                    // The IP is accessible if we can successfully get a response.
                    return true;
                }
            }
            catch (Exception)
            {
                // The IP is not accessible if an exception is thrown.
            }

            return false;
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

        public static byte[] RemoveElementsFromStart(byte[] byteArray, int elementsToRemove)
        {
            if (elementsToRemove >= 0 && elementsToRemove <= byteArray.Length)
            {
                // Calculate the length of the remaining data
                int remainingLength = byteArray.Length - elementsToRemove;

                // Create a new byte array for the remaining data
                byte[] remainingData = new byte[remainingLength];

                // Copy the remaining data from the original byte array
                Array.Copy(byteArray, elementsToRemove, remainingData, 0, remainingLength);

                return remainingData;
            }
            else
                // Invalid index, return the original byte array
                return byteArray;
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
                                {
                                    return address.ToString();
                                }
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
                                {
                                    return address.ToString();
                                }
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
                ServerConfiguration.LogError(ex);
                return fallback;
            }
        }

        /// <summary>
		/// Check a string for containing a something from list of patterns
		/// </summary>
		/// <param name="What">What string should be checked</param>
		/// <param name="For">Pattern to find</param>
		/// <param name="CaseInsensitive">Ignore character case when checking</param>
		public static bool CheckString(string What, string[] For, bool CaseInsensitive = false)
        {
            if (CaseInsensitive)
            {
                foreach (string str in For) { if (What.Contains(str, StringComparison.InvariantCultureIgnoreCase)) return true; }
                return false;
            }
            else
            {
                foreach (string str in For) { if (What.Contains(str)) return true; }
                return false;
            }
        }

        /// <summary>
		/// Get all inner exception messages.
		/// </summary>
		/// <param name="Ex">The upper exception.</param>
		public static string GetFullExceptionMessage(Exception Ex, bool ExcludeTopLevel = false, bool IncludeOnlyLast = false)
        {
            string msg = string.Empty;
            Exception e = ExcludeTopLevel ? Ex.InnerException : Ex;
            while (e != null)
            {
                if (IncludeOnlyLast) msg = e.Message;
                else msg += e.Message + "\n";
                e = e.InnerException;
            }
            return msg;
        }

        private bool IsPortOpen(string host, int port)
        {
            TcpConnectionInformation[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections();

            foreach (TcpConnectionInformation connection in tcpConnections)
            {
                if (connection.LocalEndPoint.Port == port && connection.RemoteEndPoint.Address.ToString() == host)
                    return true;
            }

            return false;
        }

        public static byte[] ReadBinaryFile(string filePath, int offset, int length)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                byte[] data = new byte[length];
                fs.Read(data, 0, length);

                return data;
            }
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

        public static byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
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

        public static int FindDataPosInBinary(byte[] data1, byte[] data2)
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
                {
                    return i;
                }
            }

            return -1; // Data2 not found in Data1
        }

        public static byte[] Combinebytearay(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static bool FindbyteSequence(byte[] byteArray, byte[] sequenceToFind)
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

        public static object[] ExecuteLuaScript(string luaScript)
        {
            using (Lua lua = new Lua())
            {
                try
                {
                    // Execute the Lua script
                    object[] returnValues = lua.DoString(luaScript);

                    // If the script returns no values, return an empty object array
                    if (returnValues == null || returnValues.Length == 0)
                        return new object[0];

                    return returnValues;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during script execution
                    Console.WriteLine("Error executing Lua script: " + ex.Message);
                    return new object[0];
                }
            }
        }

        public static byte[] ReverseByteArray(byte[] input)
        {
            byte[] reversedArray = new byte[input.Length];
            int lastIndex = input.Length - 1;

            for (int i = 0; i < input.Length; i++)
            {
                reversedArray[i] = input[lastIndex - i];
            }

            return reversedArray;
        }

        public static bool IsWindows()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT;
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

        /// <summary>
        /// Check a process for idle state (long period of no CPU load) and kill if it's idle.
        /// </summary>
        /// <param name="Proc">The process.</param>
        /// <param name="AverageLoad">Average CPU load by the process.</param>
        public static void PreventProcessIdle(ref Process Proc, ref float AverageLoad)
        {
            AverageLoad = (float)(AverageLoad + GetUsage(Proc)) / 2;

            if (!Proc.HasExited)
                if (Math.Round(AverageLoad, 6) <= 0 && !Proc.HasExited)
                {
                    //the process is counting crows. Fire!
                    Proc.Kill();
                    if (Console.GetCursorPosition().Left > 0) Console.WriteLine();
                    ServerConfiguration.LogInfo(" Idle process {0} killed.", Proc.ProcessName);
                }
        }

#pragma warning disable CA1416
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
		/// Get CPU load for process.
		/// </summary>
		/// <param name="process">The process.</param>
		/// <returns>CPU usage in percents.</returns>
		internal static double GetUsage(Process process)
        {
            //thx to: https://stackoverflow.com/a/49064915/7600726
            //see also https://www.mono-project.com/archived/mono_performance_counters/

            if (process.HasExited) return double.MinValue;

            // Preparing variable for application instance name
            string name = "";

            foreach (string instance in new PerformanceCounterCategory("Process").GetInstanceNames())
            {
                if (process.HasExited) return double.MinValue;
                if (instance.StartsWith(process.ProcessName))
                {
                    using (PerformanceCounter processId = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        if (process.Id == (int)processId.RawValue)
                        {
                            name = instance;
                            break;
                        }
                    }
                }
            }

            PerformanceCounter cpu = new PerformanceCounter("Process", "% Processor Time", name, true);

            // Getting first initial values
            cpu.NextValue();

            // Creating delay to get correct values of CPU usage during next query
            Thread.Sleep(500);

            if (process.HasExited) return double.MinValue;
            return Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);
        }
#pragma warning restore CA1416
    }
}