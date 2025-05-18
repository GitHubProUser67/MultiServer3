using Blaze3SDK;
using BlazeCommon;
using CustomLogger;
using MultiSocks.Blaze.Components.Redirector;
using MultiSocks.Blaze.Components.Util;
using MultiSocks.Blaze.Util;
using MultiSocks.ProtoSSL;
using System.Net;

namespace MultiSocks.Blaze
{
    public class BlazeClass : IDisposable
    {
        private bool disposedValue;

        private BlazeServer redirector;
        private BlazeServer mainBlaze;
        private VulnerableCertificateGenerator? SSLCache = new();

        public BlazeClass(CancellationToken cancellationToken)
        {
            string domain = "gosredirector.ea.com";

            // Create Blaze Redirector servers
            redirector = Blaze3.CreateBlazeServer(domain, new IPEndPoint(IPAddress.Parse(MultiSocksServerConfiguration.ServerBindAddress), 42127), SSLCache.GetVulnerableCustomEaCert(domain, "Global Online Studio", true, true).Item3);
            // Create  Main Blaze server
            mainBlaze = Blaze3.CreateBlazeServer(domain, new IPEndPoint(IPAddress.Parse(MultiSocksServerConfiguration.ServerBindAddress), 33152), SSLCache.GetVulnerableCustomEaCert(domain, "Global Online Studio", false, false).Item3, false);


            redirector.AddComponent<RedirectorComponent>();
            mainBlaze.AddComponent<UtilComponent>();
            mainBlaze.AddComponent<AuthComponent>();

            _ = StartRedirectorServer();
            _ = StartMainBlazeServer();

            LoggerAccessor.LogInfo("Blaze Servers initiated...");
        }

        private async Task StartRedirectorServer()
        {
            //Start it!
            await redirector.Start(-1).ConfigureAwait(false);
        }

        private async Task StartMainBlazeServer()
        {
            //Start it!
            await mainBlaze.Start(-1).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    redirector.Stop();
                    mainBlaze.Stop();

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
