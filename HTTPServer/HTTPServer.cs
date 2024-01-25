// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
using BackendProject.MiscUtils;
using CustomLogger;
using HTTPServer.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace HTTPServer
{
    public class HttpServer
    {
        #region Fields

        private readonly ConcurrentDictionary<int, TcpListener> _listeners = new();
        private CancellationTokenSource _cts = null!;
        private HttpProcessor Processor;

        #endregion

        #region Public Methods
        public HttpServer(int[] ports, List<Route> routes, CancellationToken cancellationToken)
        {
            Processor = new HttpProcessor();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (Route route in routes)
            {
                Processor.AddRoute(route);
            }

            Parallel.ForEach(ports, port => { if (VariousUtils.IsTCPPortAvailable(port)) new Thread(() => CreateHTTPPortListener(port)).Start(); });
        }

        private void CreateHTTPPortListener(int listenerPort)
        {
            Task serverHTTP = Task.Run(async () =>
            {
                try
                {
                    TcpListener listener = new(IPAddress.Any, listenerPort);
                    listener.Start();
                    LoggerAccessor.LogInfo($"HTTP Server initiated on port: {listenerPort}...");
                    _listeners.TryAdd(listenerPort, listener);

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                            _ = Task.Run(() => Processor.HandleClient(tcpClient));
                            Thread.Sleep(1);
                        }
                        catch (IOException ex)
                        {
                            if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                                socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                                && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                                LoggerAccessor.LogError($"[HTTPServer] - Client loop thrown an IOException: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound && VariousUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                                LoggerAccessor.LogError($"[HTTPServer] - Client loop thrown a SocketException: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound && VariousUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult != 995) LoggerAccessor.LogError($"[HTTPServer] - Client loop thrown an assertion: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound && VariousUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTPServer] - Listener failed to start with assertion: {ex}");
                }
            }, _cts.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _listeners.Values.ToList().ForEach(x => x.Stop());
            return Task.CompletedTask;
        }
        #endregion
    }
}
