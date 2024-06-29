// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using CustomLogger;
using CyberBackendLibrary.HTTP;
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

        #endregion

        #region Public Methods
        public HttpServer(List<ushort>? ports, List<Route> routes, bool keepaliveTest, CancellationToken cancellationToken)
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
                        new Thread(() => CreateHTTPPortListener(port, keepaliveTest)).Start();
                });
            }
        }

        private void CreateHTTPPortListener(ushort listenerPort, bool keepaliveTest)
        {
            _ = Processor.TryGetServerIP(listenerPort);

            Task serverHTTP = Task.Run(async () =>
            {
                try
                {
                    TcpListener listener = new(IPAddress.Any, listenerPort);
                    listener.Start();
                    LoggerAccessor.LogInfo($"[HTTP] - Server initiated on port: {listenerPort}...");
                    _listeners.TryAdd(listenerPort, listener);

                    if (keepaliveTest)
                    {
                        _ = Task.Run(() => {
                            const int timeOut = 5;
                            int tryout = 0;

                            while (true)
                            {
                                if (listener.Server.IsBound && !HTTPPingTest.PingServer($"http://127.0.0.1:{listenerPort}/!morelife").Result)
                                {
                                    tryout++;

                                    if (tryout >= timeOut)
                                    {
                                        listener.Stop();
                                        listener.Start();
                                        tryout = 0;
                                    }
                                }
                                else
                                    tryout = 0;

                                Thread.Sleep(10000);
                            }
                        });

                    }

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                            _ = Task.Run(() => Processor.HandleClient(tcpClient, listenerPort));
                            Thread.Sleep(1);
                        }
                        catch (OperationCanceledException)
                        {
                            LoggerAccessor.LogWarn($"[HTTP] - System requested a server shutdown on port: {listenerPort}...");
                        }
                        catch (IOException ex)
                        {
                            if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                                socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                                && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                                LoggerAccessor.LogError($"[HTTP] - Client loop thrown an IOException: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound && CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted
                            && ex.SocketErrorCode != SocketError.ConnectionRefused && ex.SocketErrorCode != SocketError.Interrupted)
                                LoggerAccessor.LogError($"[HTTP] - Client loop thrown a SocketException: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound && CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort)) // Check if server is closed, then, start it again.
                                listener.Start();
                            else
                                break;
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult != 995) LoggerAccessor.LogError($"[HTTP] - Client loop thrown an assertion: {ex}");
                            listener.Stop();

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
