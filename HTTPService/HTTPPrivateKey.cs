namespace MultiServer.HTTPService
{
    public class HTTPPrivateKey
    {
        public static string HTTPPrivatekey = "";
        public static void setup()
        {
            if (ServerConfiguration.HTTPPrivateKey == "")
            {
                ServerConfiguration.LogWarn("[HTTP] - No key so http encryption is disabled.");
            }
            else if (ServerConfiguration.HTTPPrivateKey == null || ServerConfiguration.HTTPPrivateKey.Length < 20)
            {
                ServerConfiguration.LogWarn("[HTTP] - key is less than 20 characters, so encryption is disabled.");

                ServerConfiguration.HTTPPrivateKey = "";
            }
            else
            {
                HTTPPrivatekey = ServerConfiguration.HTTPPrivateKey;
            }
        }
    }
}
