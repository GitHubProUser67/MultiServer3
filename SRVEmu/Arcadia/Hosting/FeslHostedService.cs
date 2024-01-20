using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;
using BackendProject.FOSSProjects.VulnerableSSLv3Server;
using BackendProject.FOSSProjects.VulnerableSSLv3Server.Crypto;
using SRVEmu.Arcadia.Storage;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Ports;
using SRVEmu.Arcadia.Handlers;

namespace SRVEmu.Arcadia.Hosting;

public class FeslHostedService
{
    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly AsymmetricKeyParameter _feslCertKey;
    private readonly Certificate _feslPubCert;
    private readonly ConcurrentBag<TcpListener> _listeners = new();
    private readonly ConcurrentBag<Task> _activeConnections = new();
    private readonly ConcurrentBag<Task?> _servers = new();
    private CancellationTokenSource _cts = null!;

    public FeslHostedService(SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
        (_feslCertKey, _feslPubCert) = new ProtoSSL().GetFeslEaCert();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        int[] listeningPorts = new int[] {
            (int)FeslGamePort.BeachPS3,
            (int)FeslGamePort.BadCompanyPS3,
            (int)FeslGamePort.RomePS3,
            (int)FeslServerPort.RomePC
        };

        foreach (int port in listeningPorts)
        {
            CreateFeslPortListener(port);
        }

        return Task.CompletedTask;
    }

    private void CreateFeslPortListener(int listenerPort)
    {
        Task serverFesl = Task.Run(() =>
        {
            TcpListener listener = new(IPAddress.Parse(SRVEmuServerConfiguration.ListenAddress), listenerPort);
            listener.Start();
            CustomLogger.LoggerAccessor.LogInfo("[Arcadia] - FeslHostedService-CreateFeslPortListener Fesl listening on {address}:{port}", SRVEmuServerConfiguration.ListenAddress, listenerPort);
            _listeners.Add(listener);

            while (!_cts.Token.IsCancellationRequested)
            {
                _activeConnections.Add(Task.Run(async () => await HandleConnection(await listener.AcceptTcpClientAsync(_cts.Token), listenerPort), _cts.Token));
            }
        }, _cts.Token);

        _servers.Add(serverFesl);
    }

    private async Task HandleConnection(TcpClient tcpClient, int connectionPort)
    {
        string clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString()! ?? throw new NullReferenceException("ClientEndpoint cannot be null!");
        CustomLogger.LoggerAccessor.LogInfo("[Arcadia] - FeslHostedService-HandleConnection Opening connection from: {clientEndpoint} to {port}", clientEndpoint, connectionPort);

        TlsServerProtocol serverProtocol = new(tcpClient.GetStream());
        serverProtocol.Accept(new Ssl3TlsServer(new Rc4TlsCrypto(false), _feslPubCert, _feslCertKey));

        switch (connectionPort)
        {
            case (int)FeslGamePort.BeachPS3:
            case (int)FeslGamePort.RomePS3:
            case (int)FeslGamePort.BadCompanyPS3:
                await new FeslClientHandler(new EAConnection(), _sharedCounters, _sharedCache).HandleClientConnection(serverProtocol,
                    clientEndpoint, (FeslGamePort)connectionPort);
                break;

            case (int)FeslServerPort.RomePC:
                await new FeslServerHandler(new EAConnection(), _sharedCounters, _sharedCache).HandleClientConnection(serverProtocol, clientEndpoint);
                break;
        }

        tcpClient.Close();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _listeners.ToList().ForEach(x => x.Stop());
        return Task.CompletedTask;
    }
}