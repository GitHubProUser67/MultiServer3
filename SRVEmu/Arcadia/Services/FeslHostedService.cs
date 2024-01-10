using System.Collections.Concurrent;
using System.Net.Sockets;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.Handlers;
using BackendProject.FOSSProjects.VulnerableSSLv3Server;
using BackendProject.FOSSProjects.VulnerableSSLv3Server.Crypto;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;

namespace SRVEmu.Arcadia.Services;

public class FeslHostedService
{    
    private readonly CertGenerator _certGenerator = new();

    private readonly ConcurrentBag<TcpListener> _listeners = new();
    private readonly ConcurrentBag<Task> _activeConnections = new();

    private CancellationTokenSource _cts = null!;
    private AsymmetricKeyParameter _feslCertKey = null!;
    private Certificate _feslPubCert = null!;

    private ConcurrentBag<Task?> _servers = new();

    public FeslHostedService()
    {
        
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        PrepareCertificate();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        CreateFeslPortListener(Beach.FeslPort);
        CreateFeslPortListener(BadCompany.FeslPort);
        CreateFeslPortListener(BadCompany2.FeslServerPortPc);
        CreateFeslPortListener(BadCompany2.FeslServerPortPs3);
        CreateFeslPortListener(Mercs2.FeslServerPortPC);

        return Task.CompletedTask;
    }

    private void PrepareCertificate()
    {
        string IssuerDN = "CN=OTG3 Certificate Authority, C=US, ST=California, L=Redwood City, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, emailAddress=dirtysock-contact@ea.com";
        string SubjectDN = "C=US, ST=California, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, CN=fesl.ea.com, emailAddress=fesl@ea.com";

        (_feslCertKey, _feslPubCert) = _certGenerator.GenerateProtoSSLVulnerableCert(IssuerDN, SubjectDN);
    }

    private void CreateFeslPortListener(int listenerPort)
    {
        Task serverFesl = Task.Run(async () =>
        {
            TcpListener listener = new(System.Net.IPAddress.Parse(SRVEmuServerConfiguration.ServerBindAddress), listenerPort);
            listener.Start();
            CustomLogger.LoggerAccessor.LogInfo("[fesl] - listening on {address}:{port}", SRVEmuServerConfiguration.ServerBindAddress, listenerPort);
            _listeners.Add(listener);

            while (!_cts.Token.IsCancellationRequested)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                string clientEndpoint = tcpClient.Client.RemoteEndPoint!.ToString()!;

                CustomLogger.LoggerAccessor.LogInfo("[fesl] - Opening connection from: {clientEndpoint}", clientEndpoint);

                _activeConnections.Add(Task.Run(async () => await HandleClient(tcpClient, clientEndpoint), _cts.Token));
            }
        }, _cts.Token);
        _servers.Add(serverFesl);
    }

    private async Task HandleClient(TcpClient tcpClient, string clientEndpoint)
    {
        Ssl3TlsServer connTls = new(new Rc4TlsCrypto(), _feslPubCert, _feslCertKey);
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

        FeslHandler handler = new(StaticCache._sharedCounters, StaticCache._sharedCache);
        await handler.HandleClientConnection(serverProtocol, clientEndpoint);
    }

    public Task StopAsync()
    {
        _cts.Cancel();
        _listeners.ToList().ForEach(x => x.Stop());
        return Task.CompletedTask;
    }
}