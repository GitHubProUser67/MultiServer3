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
        public HTTPServer(ConnectionHandlerDelegate connectionHandler, string host = "0.0.0.0", int port = 8080, int maxConcurrentListeners = 10)
        {
            OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            MaxConcurrentListeners = maxConcurrentListeners;
            Listener = new TcpListener(IPAddress.Parse(Host), Port);
        }
        #endregion

        #region Public Functions
        public virtual void Run(ushort ListenerPort)
        {
            if (IsRunning)
                return; //Already running, only one running instance allowed.

            IsRunning = true;
            Listener.Start();
            ExitSignal = false;

            while (!ExitSignal)
                ConnectionLooper(ListenerPort);

            TcpClientTasks.Clear();

            IsRunning = false;
        }

        #endregion

        #region Protected Functions
        protected virtual void ConnectionLooper(ushort ListenerPort)
        {
            try
            {
                while (TcpClientTasks.Count < MaxConcurrentListeners) //Maximum number of concurrent listeners
                {
                    TcpClientTasks.Add(Task.Run(async () =>
                    {
                        LoggerAccessor.LogInfo($"[HTTP] - Listening on port {ListenerPort}... (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
                        return ProcessMessagesFromClient(await Listener.AcceptTcpClientAsync(), ListenerPort);
                    }));
                }

                // Lock here to prevents concurrent modifications to the queue.
                lock (TcpClientTasks)
                {
                    // Removes crashed threads.
                    TcpClientTasks = TcpClientTasks.AsParallel().AsUnordered().Where(x => x != null).ToList();

                    Task t = Task.WhenAny(TcpClientTasks).Result;

                    if (t is Task<bool>)
                    {
                        bool? result = null;

                        try
                        {
                            result = (t as Task<bool>)?.Result;
                        }
                        catch (AggregateException ex)
                        {
                            ex.Handle(innerEx =>
                            {
                                if (innerEx is TaskCanceledException)
                                    return true; // Indicate that the exception was handled

                                LoggerAccessor.LogWarn($"[HTTP] - TcpClient Task thrown an AggregateException: {ex}");

                                return false;
                            });
                        }
                    }

                    TcpClientTasks.Remove(t);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                    && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTP] - ConnectionLooper thrown an IOException: {ex}");
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTP] - ConnectionLooper thrown a SocketException: {ex}");
            }
            catch (Exception ex)
            {
                if (ex.HResult != 995) LoggerAccessor.LogError($"[HTTP] - ConnectionLooper thrown an assertion: {ex}");
            }
        }

        protected virtual bool ProcessMessagesFromClient(TcpClient? Connection, ushort ListenerPort)
        {
            if (Connection == null)
                return false;

            using (Connection) //Auto dispose of the cilent connection
            {
                LoggerAccessor.LogInfo($"[HTTP] - Connection established on port {ListenerPort} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
                if (!Connection.Connected) //Abort if not connected
                    return false;

                OnHandleConnection.Invoke(Connection, ListenerPort);
            }
            LoggerAccessor.LogWarn($"[HTTP] - Client disconnected from port {ListenerPort} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");

            return true;
        }
        #endregion
    }
}