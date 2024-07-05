// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using CustomLogger;
using CyberBackendLibrary.HTTP;
using MozaicHTTP.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MozaicHTTP
{
    public class HttpServer
    {
        #region Fields

        private readonly ConcurrentDictionary<int, TcpListener> _listeners = new();
        private readonly CancellationTokenSource _cts = null!;

        #endregion

        #region Public Methods
        public HttpServer(Dictionary<string, bool>? serverParams, List<Route> routes, CancellationToken cancellationToken)
        {
			LoggerAccessor.LogWarn("[HTTP] - HTTP system is initialising, service will be available when initialized...");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			
            if (serverParams != null)
            {
                Parallel.ForEach(serverParams, serverParam =>
                {
                    if (ushort.TryParse(serverParam.Key, out ushort listenerPort))
                    {
                        HttpProcessor Processor = new(serverParam.Value ? new System.Security.Cryptography.X509Certificates.X509Certificate2(MozaicHTTPConfiguration.HTTPSCertificateFile, MozaicHTTPConfiguration.HTTPSCertificatePassword) : null);

                        foreach (Route route in routes)
                        {
                            Processor.AddRoute(route);
                        }

                        if (CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(listenerPort))
                            new Thread(() => CreateHTTPPortListener(Processor, listenerPort)).Start();
                    }
                });
            }
        }

        private void CreateHTTPPortListener(HttpProcessor Processor, ushort listenerPort)
        {
            _ = Processor.TryGetServerIP(listenerPort);

            Task serverHTTP = Task.Run(async () =>
            {
                try
                {
                    TcpListener listener = new(IPAddress.Any, listenerPort);
                    listener.Start();
                    LoggerAccessor.LogInfo($"[HTTP] - {(Processor.sslCertificate != null ? "Secure " : string.Empty)}Server initiated on port: {listenerPort}...");
                    _listeners.TryAdd(listenerPort, listener);

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
