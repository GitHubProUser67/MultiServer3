using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;

namespace PSMultiServer
{
    public class Misc
    {
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
        public static byte[] ObjectToByteArray(object obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
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
        public static string CreateSessionID(HttpListenerContext context)
        {
            return Guid.NewGuid().ToString();
        }
        public static string GenerateUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
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

    public class FileWriteOperation
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
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