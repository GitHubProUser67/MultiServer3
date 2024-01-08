using BackendProject;
using BackendProject.FOSSProjects.VulnerableSSLv3Server;
using BackendProject.FOSSProjects.VulnerableSSLv3Server.Crypto;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace STOMPServer
{
    public class STOMPProcessor
    {
        public static bool IsStarted = false;

        private readonly CertGenerator _certGenerator = new();

        private AsymmetricKeyParameter _rabbitCertKey = null!;

        private Certificate _rabbitPubCert = null!;

        private readonly ConcurrentBag<TcpListener> _listeners = new();
        private readonly ConcurrentBag<Task> _activeConnections = new();

        private CancellationTokenSource _cts = null!;

        private ConcurrentBag<Task?> _servers = new();

        public Task StartSTOMP(CancellationToken cancellationToken)
        {
            PrepareCertificate();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            CreateTCPPortListener(10086);

            return Task.CompletedTask;
        }

        private void CreateTCPPortListener(int listenerPort)
        {
            Task serverSTOMP = Task.Run(async () =>
            {
                TcpListener listener = new(System.Net.IPAddress.Any, listenerPort);
                listener.Start();
                CustomLogger.LoggerAccessor.LogInfo("[STOMP] - listening on 0.0.0.0:{port}", listenerPort);
                _listeners.Add(listener);

                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                    string clientEndpoint = tcpClient.Client.RemoteEndPoint!.ToString()!;

                    CustomLogger.LoggerAccessor.LogInfo("[STOMP] - Opening connection from: {clientEndpoint}", clientEndpoint);

                    Task connection = Task.Run(async () => await HandleClient(tcpClient, clientEndpoint), _cts.Token);
                    _activeConnections.Add(connection);
                }
            }, _cts.Token);
            _servers.Add(serverSTOMP);
        }

        private async Task HandleClient(TcpClient tcpClient, string clientEndpoint)
        {
            Ssl3TlsServer connTls = new(new Rc4TlsCrypto(), _rabbitPubCert, _rabbitCertKey);
            TlsServerProtocol serverProtocol = new(tcpClient.GetStream());

            try
            {
                serverProtocol.Accept(connTls);
            }
            catch (Exception e)
            {
                CustomLogger.LoggerAccessor.LogError("Failed to accept connection: {message}", e.Message);

                serverProtocol.Flush();
                serverProtocol.Close();
                connTls.Cancel();

                return;
            }

            while (serverProtocol.IsConnected)
            {
                int read;
                byte[]? readBuffer;

                try
                {
                    (read, readBuffer) = await ReadApplicationDataAsync(serverProtocol);
                }
                catch
                {
                    CustomLogger.LoggerAccessor.LogWarn("Connection has been closed with {endpoint}", clientEndpoint);
                    break;
                }

                if (read == 0)
                    continue;

                CustomLogger.LoggerAccessor.LogInfo($"[STOMP] - Debug Data: {MiscUtils.ByteArrayToHexString(readBuffer)}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _listeners.ToList().ForEach(x => x.Stop());
            return Task.CompletedTask;
        }

        public static async Task<(int, byte[])> ReadApplicationDataAsync(TlsServerProtocol network)
        {
            byte[] readBuffer = new byte[8096];
            try
            {
                int read = await Task.Run(() => network.ReadApplicationData(readBuffer, 0, readBuffer.Length));
                return (read, readBuffer);
            }
            catch
            {
                throw new Exception("[STOMP] - Connection has been closed");
            }
        }

        private void PrepareCertificate()
        {
            string IssuerDN = "CN=MultiServer Certificate Authority, C=US, ST=California, L=Redwood City, O=\"MultiServer Corp\", OU=Online Technology Group, emailAddress=whatever@gmail.com";
            string SubjectDN = "C=US, ST=California, O=\"MultiServer Corp\", OU=Online Technology Group, CN=prod.homemq.online.scee.com, emailAddress=whatever@gmail.com";

            (_rabbitCertKey, _rabbitPubCert) = _certGenerator.GenerateVulnerableCert(IssuerDN, SubjectDN, SSLUtils.HOME_PRIVATE_KEY);
        }
    }
}
