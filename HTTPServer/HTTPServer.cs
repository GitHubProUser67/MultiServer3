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

            Parallel.ForEach(ports, port => { new Thread(() => CreateHTTPPortListener(port)).Start(); }) ;
        }

        private async Task TestHttpConnectionAsync(int listenerPort) // TCP Listener user space is tricky, sometimes the port silently fails to bind, causing issues. So we test.
        {
            Thread.Sleep(1000); // We give the server a bit of time to initialize.

            for (int retryCount = 0; retryCount < 3; retryCount++)
            {
                try
                {
                    HttpClient client = new();
                    // Set a timeout of 5 seconds (adjust as needed)
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{listenerPort}/testconn/");
                    response.EnsureSuccessStatusCode(); // Ensure the request was successful
                    if (await response.Content.ReadAsStringAsync() == "Youhavereachedyourdestination")
                        return;
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"Failed to send test HTTP test request: {ex}");
                }
            }

            if (_listeners.TryRemove(listenerPort, out TcpListener? listener))
            {
                try
                {
                    listener.Stop();
                    LoggerAccessor.LogError($"HTTP Server on port {listenerPort} failed the test, so removed and stopped. Server will restart on it's own.");
                    new Thread(() => CreateHTTPPortListener(listenerPort)).Start();
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"Error while stopping and removing HTTP Server, thread will permently abort woth exception: {ex}");
                }
            }
            else
                LoggerAccessor.LogWarn($"No listener found for port {listenerPort}.");
        }

        private void CreateHTTPPortListener(int listenerPort)
        {
            Task serverSTOMP = Task.Run(async () =>
            {
                try
                {
                    TcpListener listener = new(IPAddress.Any, listenerPort);
                    listener.Start();
                    LoggerAccessor.LogInfo($"HTTP Server initiated on port: {listenerPort}...");
                    _listeners.TryAdd(listenerPort, listener);

                    _ = TestHttpConnectionAsync(listenerPort);

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            TcpClient tcpClient = await listener.AcceptTcpClientAsync(_cts.Token);
                            _ = Task.Run(() => Processor.HandleClient(tcpClient));
                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult != 995) LoggerAccessor.LogError($"[HTTPServer] - Client loop thrown an assertion: {ex}");
                            listener.Stop();

                            if (!listener.Server.IsBound) // Check if server is closed, then, start it again.
                                listener.Start();
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
