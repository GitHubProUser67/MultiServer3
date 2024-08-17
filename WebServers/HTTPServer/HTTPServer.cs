// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using CustomLogger;
using HTTPServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public class HttpServer
    {
        #region Fields

        private readonly ConcurrentDictionary<int, TcpListener> _listeners = new();
        private readonly CancellationTokenSource _cts = null!;
        private readonly HttpProcessor Processor;
        protected readonly int AwaiterTimeoutInMS = 500; // The max time to wait between ExitSignal checks
        protected readonly int MaxConcurrentListeners = Environment.ProcessorCount * 64;

        #endregion

        #region Public Methods
        public HttpServer(List<ushort>? ports, List<Route> routes, CancellationToken cancellationToken)
        {
            LoggerAccessor.LogWarn("[HTTP] - HTTP system is initialising, service will be available when initialized...");

            Processor = new HttpProcessor();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (Route route in routes)
            {
                Processor.AddRoute(route);
            }
			
            if (ports != null)
            {
                Parallel.ForEach(ports, port =>
                {
                    if (CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(port))
                        new Thread(() => CreateHTTPPortListener(port)).Start();
                    else
                        LoggerAccessor.LogError($"[HTTP] - port {port} is not avaialable, server failed to start!");
                });
            }
        }

        private void CreateHTTPPortListener(ushort listenerPort)
        {
            _ = Processor.TryGetServerIP(listenerPort);

            _ = Task.Run(() =>
            {
                List<Task> _clientTasks = new();

                try
                {
                    TcpListener listener = new(IPAddress.Any, listenerPort);
                    listener.Start();
                    LoggerAccessor.LogInfo($"[HTTP] - Server initiated on port: {listenerPort}...");
                    _listeners.TryAdd(listenerPort, listener);

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            while (_clientTasks.Count < MaxConcurrentListeners) // Maximum number of concurrent listeners
                            {
                                _clientTasks.Add(Task.Run(async () =>
                                {
                                    Processor.HandleClient(await listener.AcceptTcpClientAsync(_cts.Token), listenerPort);
                                }));
                            }

                            int RemoveAtIndex = Task.WaitAny(_clientTasks.ToArray(), AwaiterTimeoutInMS, _cts.Token); // Synchronously Waits up to 500ms for any Task completion
                            if (RemoveAtIndex > 0) // Remove the completed task from the list
                                _clientTasks.RemoveAt(RemoveAtIndex);
                        }
                        catch (OperationCanceledException)
                        {
                            LoggerAccessor.LogWarn($"[HTTP] - System requested a server shutdown on port: {listenerPort}...");

                            _clientTasks.Clear();
                        }
                        catch (IOException ex)
                        {
                            if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                                socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                                && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                                LoggerAccessor.LogError($"[HTTP] - Client loop thrown an IOException: {ex}");
                            listener.Stop();

                            _clientTasks.Clear();

                            if (!listener.Server.IsBound && CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                                LoggerAccessor.LogError($"[HTTP] - Client loop thrown a SocketException: {ex}");
                            listener.Stop();

                            _clientTasks.Clear();

                            if (!listener.Server.IsBound && CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult != 995) LoggerAccessor.LogError($"[HTTP] - Client loop thrown an assertion: {ex}");
                            listener.Stop();

                            _clientTasks.Clear();

                            if (!listener.Server.IsBound && CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTP] - Listener failed to start with assertion: {ex}");
                }

            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            _listeners.Values.ToList().ForEach(x => x.Stop());
        }
        #endregion
    }
}
