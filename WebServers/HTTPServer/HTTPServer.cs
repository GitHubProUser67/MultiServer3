using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServer
{
    public class HTTPServer
    {

        #region Public Properties
        private volatile bool _ExitSignal;
        public virtual bool ExitSignal
        {
            get => _ExitSignal;
            set => _ExitSignal = value;
        }
        #endregion

        #region Public Delegates
        public delegate void ConnectionHandlerDelegate(TcpClient tcpClient, ushort ListenerPort);
        public delegate void MessageDelegate(string message);
        #endregion

        #region Variables

        #region Init/State
        protected readonly int AwaiterTimeoutInMS; //The max time to wait between ExitSignal checks
        protected readonly string Host;
        protected readonly int Port;
        protected readonly int MaxConcurrentListeners;
        protected readonly TcpListener Listener;
        
        protected bool IsRunning;
        protected List<Task> TcpClientTasks = new List<Task>();
        #endregion

        #region Callbacks
        protected readonly ConnectionHandlerDelegate OnHandleConnection; //the connection handler logic will be performed by the consumer of class
        #endregion
        #endregion

        #region Constructor
        public HTTPServer(ConnectionHandlerDelegate connectionHandler, string host = "0.0.0.0", int port = 8080, int maxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {
            OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            MaxConcurrentListeners = maxConcurrentListeners;
            AwaiterTimeoutInMS = awaiterTimeoutInMS;
            Listener = new TcpListener(IPAddress.Parse(Host), Port);
        }
        #endregion

        #region Public Functions
        public virtual void Run()
        {
            if (IsRunning)
            {
                LoggerAccessor.LogWarn($"[HTTP] - Server already active on port {Port}.");
                return; //Already running, only one running instance allowed.
            }

            try
            {
                IsRunning = true;
                Listener.Start();
                ExitSignal = false;

                LoggerAccessor.LogInfo($"[HTTP] - Server started on port {Port}...");

                while (!ExitSignal)
                    ConnectionLooper();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[HTTP] - An Exception Occured while starting the http server: " + e.Message);
            }

            TcpClientTasks.Clear();

            IsRunning = false;
        }

        #endregion

        #region Protected Functions
        protected virtual void ConnectionLooper()
        {
            while (TcpClientTasks.Count < MaxConcurrentListeners) //Maximum number of concurrent listeners
            {
                TcpClientTasks.Add(Task.Run(async () =>
                {
#if DEBUG
                    LoggerAccessor.LogInfo($"[HTTP] - Listening on port {Port}... (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif
                    return ProcessMessagesFromClient(await Listener.AcceptTcpClientAsync().ConfigureAwait(false));
                }));
            }

            int RemoveAtIndex = Task.WaitAny(TcpClientTasks.ToArray(), AwaiterTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
            if (RemoveAtIndex != -1) //Remove the completed task from the list
                TcpClientTasks.RemoveAt(RemoveAtIndex);
        }

        protected virtual bool ProcessMessagesFromClient(TcpClient? Connection)
        {
            if (Connection == null)
                return false;

            using (Connection) //Auto dispose of the cilent connection
            {
#if DEBUG
                LoggerAccessor.LogInfo($"[HTTP] - Connection established on port {Port} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif
                if (!Connection.Connected) //Abort if not connected
                    return false;

                OnHandleConnection.Invoke(Connection, (ushort)Port);
            }
#if DEBUG
            LoggerAccessor.LogWarn($"[HTTP] - Client disconnected from port {Port} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif
            return true;
        }
        #endregion
    }
}