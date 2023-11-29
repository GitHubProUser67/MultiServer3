// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
using CustomLogger;
using HTTPServer.Models;
using System.Net;
using System.Net.Sockets;

namespace HTTPServer
{
    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener? Listener;
        private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        #region Public Methods
        public HttpServer(int port, List<Route> routes)
        {
            Port = port;
            Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            LoggerAccessor.LogInfo($"HTTP Server initiated on port: {Port}...");
            while (IsActive)
            {
                try
                {
                    TcpClient tcpClient = Listener.AcceptTcpClient();

                    Thread thread = new(() =>
                    {
                        Processor.HandleClient(tcpClient);
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTP] - Listen thrown an exception : {ex}");
                }
            }
        }
        #endregion
    }
}
