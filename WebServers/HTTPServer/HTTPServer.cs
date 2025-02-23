using CustomLogger;
using NetworkLibrary.TCP_IP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public class HTTPServer
    {
        #region Public Delegates
        public delegate void ConnectionHandlerDelegate(TcpClient tcpClient, ushort ListenerPort);
        #endregion

        public static bool IsStarted = false;

        private Thread? thread;
        private volatile bool threadActive;

        private TcpListener? listener;

        private List<Task> TcpClientTasks = new();

        private readonly int AwaiterTimeoutInMS;
        private readonly int MaxConcurrentListeners;

        private readonly string host;
        private readonly ushort port;

        #region Callbacks
        private readonly ConnectionHandlerDelegate OnHandleConnection;
        #endregion

        public HTTPServer(ConnectionHandlerDelegate connectionHandler, string host, ushort port, int MaxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {
            OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.port = port;
            this.MaxConcurrentListeners = MaxConcurrentListeners;
            AwaiterTimeoutInMS = awaiterTimeoutInMS;

            Start();
        }

        public void Start()
        {
            if (thread != null)
            {
                LoggerAccessor.LogWarn("[HTTP] - Server already active.");
                return;
            }
            thread = new Thread(Listen);
            thread.Start();
            IsStarted = true;
        }

        public void Stop()
        {
            // stop thread and listener
            threadActive = false;
            if (listener != null) listener.Stop();

            // wait for thread to finish
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            // finish closing listener
            if (listener != null)
                listener = null;

            IsStarted = false;
        }

        private void Listen()
        {
            threadActive = true;

            object _sync = new();

            // start listener
            try
            {
                listener = new TcpListener(IPAddress.Parse(IPUtils.GetFirstActiveIPAddress(host, "0.0.0.0")), port);
                listener.Start();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[HTTP] - An Exception Occured while starting the http server: " + e.Message);
                threadActive = false;
                return;
            }

            LoggerAccessor.LogInfo($"[HTTP] - Server started on port {port}...");

            // wait for requests
            while (threadActive)
            {
                lock (_sync)
                {
                    if (!threadActive)
                        break;
                }

                while (TcpClientTasks.Count < MaxConcurrentListeners) //Maximum number of concurrent listeners
                    TcpClientTasks.Add(listener.AcceptTcpClientAsync().ContinueWith(t =>
                    {
                        TcpClient? client = null;
                        try
                        {
                            if (!t.IsCompleted)
                                return;
                            client = t.Result;
                        }
                        catch
                        {
                        }
                        _ = Task.Run(() => { ProcessMessagesFromClient(client); });
                    }));

                int RemoveAtIndex = Task.WaitAny(TcpClientTasks.ToArray(), AwaiterTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
                if (RemoveAtIndex != -1) //Remove the completed task from the list
                    TcpClientTasks.RemoveAt(RemoveAtIndex);
            }
        }

        #region Protected Functions
        protected virtual bool ProcessMessagesFromClient(TcpClient? Connection)
        {
            if (Connection == null)
                return false;
#if DEBUG
            LoggerAccessor.LogInfo($"[HTTP] - Connection established on port {port} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif
            using (Connection) //Auto dispose of the cilent connection
            {
                if (Connection.Connected)
                    OnHandleConnection.Invoke(Connection, port);
            }
#if DEBUG
            LoggerAccessor.LogWarn($"[HTTP] - Client disconnected from port {port} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif

            return true;
        }
        #endregion
    }
}