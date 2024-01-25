using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Ports;
using SRVEmu.Arcadia.Handlers;
using SRVEmu.Arcadia.Storage;

namespace SRVEmu.Arcadia.Hosting;

public class TheaterHostedService
{
    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly ConcurrentBag<TcpListener> _listeners = new();
    private readonly ConcurrentBag<Task> _activeConnections = new();
    private readonly ConcurrentBag<Task?> _servers = new();
    private CancellationTokenSource _cts = null!;

    public TheaterHostedService(SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var listeningPorts = new int[] {
            (int)TheaterGamePort.BeachPS3,
            (int)TheaterGamePort.BadCompanyPS3,
            (int)TheaterGamePort.RomePS3,
            (int)TheaterGamePort.RomePC,
            (int)TheaterServerPort.RomePC
        };

        foreach (int port in listeningPorts)
        {
            CreateTheaterPortListener(port);
        }

        return Task.CompletedTask;
    }

    private void CreateTheaterPortListener(int listenerPort)
    {
        Task server = Task.Run(() =>
        {
            TcpListener listener = new(IPAddress.Parse(SRVEmuServerConfiguration.ListenAddress), listenerPort);
            listener.Start();
            CustomLogger.LoggerAccessor.LogInfo("[Arcadia] - TheatreHostedService-CreateTheaterPortListener Theater listening on {address}:{port}", SRVEmuServerConfiguration.ListenAddress, listenerPort);
            _listeners.Add(listener);

            while (!_cts.Token.IsCancellationRequested)
            {
                _activeConnections.Add(Task.Run(async () => await HandleConnection(await listener.AcceptTcpClientAsync(_cts.Token), listenerPort), _cts.Token));
            }
        });

        _servers.Add(server);
    }

    private async Task HandleConnection(TcpClient tcpClient, int connectionPort)
    {
        string clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString()! ?? throw new NullReferenceException("ClientEndpoint cannot be null!");
        CustomLogger.LoggerAccessor.LogInfo("[Arcadia] - TheatreHostedService-HandleConnection Opening connection from: {clientEndpoint} to {port}", clientEndpoint, connectionPort);

        var networkStream = tcpClient.GetStream();

        switch (connectionPort)
        {
            case (int)TheaterGamePort.RomePC:
            case (int)TheaterGamePort.BeachPS3:
            case (int)TheaterGamePort.RomePS3:
            case (int)TheaterGamePort.BadCompanyPS3:
                await new TheaterClientHandler(new EAConnection(), _sharedCounters, _sharedCache).HandleClientConnection(networkStream, clientEndpoint);
                break;

            case (int)TheaterServerPort.RomePC:
                await new TheaterServerHandler(new EAConnection(), _sharedCounters, _sharedCache).HandleClientConnection(networkStream, clientEndpoint);
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