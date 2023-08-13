using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using NLua;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Diagnostics;

namespace PSMultiServer
{
    public class Misc
    {
        public static string GetFirstActiveIPAddress(string hostName)
        {
            try
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

                return string.Empty;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
                return string.Empty;
            }
        }

        private bool IsPortOpen(string host, int port)
        {
            TcpConnectionInformation[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections();

            foreach (TcpConnectionInformation connection in tcpConnections)
            {
                if (connection.LocalEndPoint.Port == port && connection.RemoteEndPoint.Address.ToString() == host)
                {
                    return true;
                }
            }

            return false;
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
                    {
                        return new object[0];
                    }

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
    public static class UniqueNumberGenerator
    {
        // Function to generate a unique number based on a string using MD5
        public static int GenerateUniqueNumber(string inputString)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes("0HS0000000000000A" + inputString));

                // To get a small integer within Lua int bounds, take the least significant 16 bits of the hash and convert to int16
                int uniqueNumber = Math.Abs(BitConverter.ToUInt16(data, 0));

                return uniqueNumber;
            }
        }
    }
    public static class ScoreboardNameGenerator
    {
        private static Random random = new Random();

        // List of silly French-sounding words to be used in the names
        private static string[] sillyFrenchWords = { "Croissant", "Baguette", "Fougasse", "TarteAuFromage", "Tabernack", "UnePetiteContine", "ChuckNorris", "Pamplemousse", "JimCarrey", "Fromage" };

        public static string GenerateRandomName()
        {
            return sillyFrenchWords[random.Next(0, sillyFrenchWords.Length)];
        }
    }

    // Custom comparer to sort file names numerically
    public class SlotFileComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int slotNumberX = int.Parse(Path.GetFileNameWithoutExtension(x));
            int slotNumberY = int.Parse(Path.GetFileNameWithoutExtension(y));

            return slotNumberX.CompareTo(slotNumberY);
        }
    }
}