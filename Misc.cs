using System.Net;
using System.Net.NetworkInformation;
using System.Text;

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
            System.Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            System.Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
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
        public static string CreateSessionID(HttpListenerContext context)
        {
            return Guid.NewGuid().ToString();
        }
        public static string Base64Decode(string base64EncodedText)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
