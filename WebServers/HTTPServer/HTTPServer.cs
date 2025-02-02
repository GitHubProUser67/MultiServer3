namespace HTTPServer
{
    using CustomLogger;
    using NetworkLibrary.Extension.Csharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

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
        protected readonly MessageDelegate OnMessage;
        #endregion
        #endregion

        #region Constructor
        public HTTPServer(MessageDelegate onMessage, ConnectionHandlerDelegate connectionHandler, 
                            string host = "0.0.0.0", int port = 8080, int maxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {
            OnMessage = onMessage ?? throw new ArgumentNullException(nameof(onMessage));
            OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            MaxConcurrentListeners = maxConcurrentListeners;
            AwaiterTimeoutInMS = awaiterTimeoutInMS;
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
            {
                try
                {
                    if (ExitSignal) break;
					
                    ConnectionLooper(ListenerPort);
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

            IsRunning = false;
        }

        #endregion

        #region Protected Functions
        protected virtual void ConnectionLooper(ushort ListenerPort)
        {
            while (TcpClientTasks.Count < MaxConcurrentListeners) //Maximum number of concurrent listeners
            {
                var AwaiterTask = Task.Run(async () =>
                {
                    OnMessage.Invoke("Listening... on Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
                    ProcessMessagesFromClient(await Listener.AcceptTcpClientAsync(), ListenerPort);
                });
                TcpClientTasks.Add(AwaiterTask);
            }
            int RemoveAtIndex = Task.WaitAny(TcpClientTasks.ToArray(), AwaiterTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
            if (RemoveAtIndex > 0) //Remove the completed task from the list
                TcpClientTasks.RemoveAt(RemoveAtIndex);
        }

        protected virtual void ProcessMessagesFromClient(TcpClient? Connection, ushort ListenerPort)
        {
            if (Connection == null)
                return;

            using (Connection) //Auto dispose of the cilent connection
            {
                OnMessage.Invoke("Connection established on Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
                if (!Connection.Connected) //Abort if not connected
                    return;

                OnHandleConnection.Invoke(Connection, ListenerPort);
            }
            OnMessage.Invoke("client disconnected from Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
        }
        #endregion
    }
}