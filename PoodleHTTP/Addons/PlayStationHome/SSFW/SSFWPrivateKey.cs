namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW
{
    public class SSFWPrivateKey
    {
        public static string SSFWPrivatekey = "";
        public static void setup()
        {
            if (ServerConfiguration.SSFWPrivateKey == "")
            {
                ServerConfiguration.LogWarn("[SSFW] - No key so ssfw encryption is disabled.");
            }
            else if (ServerConfiguration.SSFWPrivateKey == null || ServerConfiguration.SSFWPrivateKey.Length < 20)
            {
                ServerConfiguration.LogWarn("[SSFW] - key is less than 20 characters, so encryption is disabled.");

                ServerConfiguration.SSFWPrivateKey = "";
            }
            else
            {
                SSFWPrivatekey = ServerConfiguration.SSFWPrivateKey;
            }
        }
    }
}
