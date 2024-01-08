// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
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

        private readonly ConcurrentBag<TcpListener> _listeners = new();
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

            Parallel.ForEach(ports, port => { CreateHTTPPortListener(port); }) ;
        }

        private void CreateHTTPPortListener(int listenerPort)
        {
            Task serverSTOMP = Task.Run(async () =>
            {
                TcpListener listener = new(IPAddress.Any, listenerPort);
                listener.Start();
                LoggerAccessor.LogInfo($"HTTP Server initiated on port: {listenerPort}...");
                _listeners.Add(listener);

                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                    Thread thread = new(() =>
                    {
                        Processor.HandleClient(tcpClient);
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
            }, _cts.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _listeners.ToList().ForEach(x => x.Stop());
            return Task.CompletedTask;
        }
        #endregion
    }
}
