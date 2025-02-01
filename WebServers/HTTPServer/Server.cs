namespace HTTPServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class Server
    {

        #region Public Properties
        private volatile bool _ExitSignal;
        public virtual bool ExitSignal
        {
            get => this._ExitSignal;
            set => this._ExitSignal = value;
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
        protected readonly ConnectionHandlerDelegate OnHandleConnection; //the connection handler logic will be performed by the consumer of this class
        protected readonly MessageDelegate OnMessage;
        #endregion
        #endregion

        #region Constructor
        public Server(MessageDelegate onMessage, ConnectionHandlerDelegate connectionHandler, 
                            string host = "0.0.0.0", int port = 8080, int maxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {
            this.OnMessage = onMessage ?? throw new ArgumentNullException(nameof(onMessage));
            this.OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.Port = port;
            this.MaxConcurrentListeners = maxConcurrentListeners;
            this.AwaiterTimeoutInMS = awaiterTimeoutInMS;
            this.Listener = new TcpListener(IPAddress.Parse(this.Host), this.Port);
        }
        #endregion

        #region Public Functions
        public virtual void Run(ushort ListenerPort)
        {
            if (this.IsRunning)
                return; //Already running, only one running instance allowed.

            this.IsRunning = true;
            this.Listener.Start();
            this.ExitSignal = false;

            while (!this.ExitSignal)
                this.ConnectionLooper(ListenerPort);

            this.IsRunning = false;
        }

        #endregion

        #region Protected Functions
        protected virtual void ConnectionLooper(ushort ListenerPort)
        {
            while (this.TcpClientTasks.Count < this.MaxConcurrentListeners) //Maximum number of concurrent listeners
            {
                var AwaiterTask = Task.Run(async () =>
                {
                    this.OnMessage.Invoke("Listening... on Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
                    this.ProcessMessagesFromClient(await this.Listener.AcceptTcpClientAsync(), ListenerPort);
                });
                this.TcpClientTasks.Add(AwaiterTask);
            }
            int RemoveAtIndex = Task.WaitAny(this.TcpClientTasks.ToArray(), this.AwaiterTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
            if (RemoveAtIndex > 0) //Remove the completed task from the list
                this.TcpClientTasks.RemoveAt(RemoveAtIndex);
        }

        protected virtual void ProcessMessagesFromClient(TcpClient? Connection, ushort ListenerPort)
        {
            if (Connection == null)
                return;

            using (Connection) //Auto dispose of the cilent connection
            {
                this.OnMessage.Invoke("Connection established on Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
                if (!Connection.Connected) //Abort if not connected
                    return;

                this.OnHandleConnection.Invoke(Connection, ListenerPort);
            }
            this.OnMessage.Invoke("client disconnected from Thread " + Thread.CurrentThread.ManagedThreadId.ToString());
        }
        #endregion
    }
}