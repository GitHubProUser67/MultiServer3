using Blaze3SDK;
using BlazeCommon;
using CustomLogger;
using MultiSocks.Blaze.Redirector;
using MultiSocks.Tls;
using System.Net;

namespace MultiSocks.Blaze
{
    public class BlazeClass : IDisposable
    {
        private bool disposedValue;

        private BlazeServer redirector;
        private VulnerableCertificateGenerator? SSLCache = new();

        public BlazeClass(CancellationToken cancellationToken)
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
            string domain = "gosredirector.ea.com";

            // Create Blaze Redirector server
            redirector = Blaze3.CreateBlazeServer(domain, new IPEndPoint(IPAddress.Any, 42127), SSLCache.GetVulnerableCustomEaCert(domain, "fesl@ea.com").Item3);
            redirector.AddComponent<RedirectorComponent>();

            _ = StartServer();

            LoggerAccessor.LogInfo("Blaze Servers initiated...");
        }

        private async Task StartServer()
        {
            //Start it!
            await redirector.Start(-1).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    redirector.Stop();

                    LoggerAccessor.LogWarn("Blaze Servers stopped...");
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~BlazeServer()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
