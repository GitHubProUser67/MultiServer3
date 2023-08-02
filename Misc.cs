using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using NLua;
using System.Security.Cryptography;

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

        public static string setipintextbox()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in interfaces)
            {
                // Check if the network interface is up and connected to the internet
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    // Get the IPv4 address if available
                    if (ipProperties != null)
                    {
                        foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                        {
                            if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return ipInfo.Address.ToString();
                            }
                        }
                    }
                }
            }

            return "*";

        }
        public static string GetLastNCharacters(string input, int n)
        {
            if (n >= input.Length)
                return input; // If n is greater than or equal to the string length, return the whole string.
            else
                return input.Substring(input.Length - n);
        }
        public static byte[] Combinebytearay(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }
        public static byte[] CombineByteArrays(byte[] array1, byte[] array2)
        {
            var combinedArray = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, combinedArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, combinedArray, array1.Length, array2.Length);
            return combinedArray;
        }
        public static byte[] ConcatenateByteArrays(byte[] array1, byte[] array2)
        {
            byte[] combinedArray = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, combinedArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, combinedArray, array1.Length, array2.Length);
            return combinedArray;
        }

        public static byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
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

        public static string ExtractBoundary(string contentType)
        {
            int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.InvariantCultureIgnoreCase);
            if (boundaryIndex != -1)
            {
                return contentType.Substring(boundaryIndex + 9);
            }
            return null;
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

        public static string CreateSessionID(HttpListenerContext context)
        {
            return Guid.NewGuid().ToString();
        }

        public static string GenerateUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
        }

        public static string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string GetFirstEightCharacters(string input)
        {
            if (input.Length >= 8)
            {
                return input.Substring(0, 8);
            }
            else
            {
                // If the input is less than 8 characters, you can handle it accordingly
                // For simplicity, let's just pad with zeros in this case
                return input.PadRight(8, '0');
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

        public static string Base64Encode(string input)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(input);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            return encodedText;
        }

        public static string Base64Decode(string base64EncodedText)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool IsWindows()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT;
        }

        public static IArchive GetArchiveReader(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".zip")
            {
                return SharpCompress.Archives.Zip.ZipArchive.Open(filePath);
            }
            else if (extension == ".rar")
            {
                return RarArchive.Open(filePath);
            }
            else if (extension == ".tar")
            {
                return TarArchive.Open(filePath);
            }
            else if (extension == ".gz")
            {
                return ArchiveFactory.Open(filePath);
            }
            else if (extension == ".7z")
            {
                return SevenZipArchive.Open(filePath);
            }

            return null;
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