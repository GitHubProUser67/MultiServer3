using System;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CyberBackendLibrary.HTTP
{
    public class HTTPPingTest
    {
        public static async Task<bool> PingServer(string url)
        {
            // Ignore SSL certificate errors
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationCallback;
#pragma warning disable // WebClient is still working better than HttpClient in NET6.0
            using WebClient client = new WebClient();
#pragma warning restore
            client.Headers.Add("Accept", "text/json");
            client.Headers.Add("User-Agent", RuntimeInformation.FrameworkDescription);

            try
            {
                return await client.DownloadStringTaskAsync(url) == "{}";
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[HTTPPingTest] - Thrown an exception: {ex}");
            }

            return false;
        }

        private static bool IgnoreCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Ignore all certificate errors
        }
    }
}
