using CustomLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPSecureServerLite
{
    public class HTTPSecureServer
    {
        #region Fields

        private readonly ConcurrentDictionary<int, HttpsProcessor> _listeners = new();
        private readonly CancellationTokenSource _cts = null!;

        #endregion

        #region Public Methods
        public HTTPSecureServer(List<ushort>? ports, CancellationToken cancellationToken)
        {
            LoggerAccessor.LogWarn("[HTTPS] - HTTPS system is initialising, service will be available when initialized...");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (ports != null)
            {
                Parallel.ForEach(ports, port =>
                {
                    if (CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsPortAvailable(port))
                        new Thread(() => CreateHTTPSPortServer(port)).Start();
                });
            }
        }

        private void CreateHTTPSPortServer(ushort listenerPort)
        {
            Task serverHTTP = Task.Run(async () =>
            {
                HttpsProcessor processor = new(HTTPSServerConfiguration.HTTPSCertificateFile, HTTPSServerConfiguration.HTTPSCertificatePassword, "0.0.0.0", listenerPort); // 0.0.0.0 as the certificate binds to this IP
                _listeners.TryAdd(listenerPort, processor);

                // Wait until cancellation is requested or task completes
                await Task.Delay(-1, _cts.Token);

            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _listeners.Values.ToList().ForEach(x => x.StopServer());
        }
        #endregion
    }
}
