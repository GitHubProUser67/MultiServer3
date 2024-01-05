using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.Handlers;

namespace SRVEmu.Arcadia.Services;

public class TheaterHostedService
{
    private readonly TcpListener _tcpListener;
    private readonly ConcurrentBag<Task> _activeConnections = new();
    private readonly UdpClient _udpListener;

    private CancellationTokenSource _cts = null!;

    private Task? _tcpServer;

    public TheaterHostedService()
    {
        IPEndPoint endpoint = new(IPAddress.Parse(SRVEmuServerConfiguration.ServerBindAddress), Beach.TheaterPort);
        _tcpListener = new TcpListener(endpoint);
        _udpListener = new UdpClient(endpoint);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _tcpListener.Start();
        _tcpServer = Task.Run(async () =>
        {
            CustomLogger.LoggerAccessor.LogInfo("[Theater] - listening on {address}:{port}", SRVEmuServerConfiguration.ServerBindAddress, Beach.TheaterPort);

            while (!_cts.Token.IsCancellationRequested)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync(_cts.Token);
                string clientEndpoint = tcpClient.Client.RemoteEndPoint!.ToString()!;

                CustomLogger.LoggerAccessor.LogInfo("[Theater] - Opening TCP connection from: {clientEndpoint}", clientEndpoint);

                Task connection = Task.Run(async () => await HandleClient(tcpClient, clientEndpoint), _cts.Token);
                _activeConnections.Add(connection);
            }
        }, _cts.Token);

        return Task.CompletedTask;
    }

    private async Task HandleClient(TcpClient tcpClient, string clientEndpoint)
    {
        NetworkStream? networkStream = tcpClient.GetStream();
        TheaterHandler handler = new(StaticCache._sharedCounters, StaticCache._sharedCache);
        await handler.HandleClientConnection(networkStream, clientEndpoint);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _tcpListener.Stop();

        return Task.CompletedTask;
    }
}