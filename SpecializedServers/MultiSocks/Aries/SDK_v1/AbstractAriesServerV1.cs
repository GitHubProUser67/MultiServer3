using CustomLogger;
using CyberBackendLibrary.DataTypes;
using MultiSocks.Aries.SDK_v1.Messages;
using MultiSocks.Tls;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiSocks.Aries.SDK_v1
{
    public abstract class AbstractAriesServerV1 : IDisposable
    {
        public abstract Dictionary<string, Type?> NameToClass { get; }
        public string? Project = null;
        public string? SKU = null;
        public string listenIP = string.Empty;
        public int SessionID = 1;
        public ProtoSSLUtils? SSLCache = null;
        public List<AriesClient> DirtySocksClients = new();
        public TcpListener? Listener;

        private bool secure = false;
        private bool WeakChainSignedRSAKey = false;
        private string CN = string.Empty;
        private string email = string.Empty;
        private Thread ListenerThread;

        public AbstractAriesServerV1(ushort port, string listenIP, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false)
        {
            this.listenIP = listenIP;
            this.secure = secure;
            this.WeakChainSignedRSAKey = WeakChainSignedRSAKey;
            this.CN = CN;
            this.email = email;
            this.Project = Project;
            this.SKU = SKU;

            if (secure)
                SSLCache = new ProtoSSLUtils();

            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();

            ListenerThread = new Thread(RunLoop);
            ListenerThread.Start();
        }

        private async void RunLoop()
        {
            try
            {
                while (true)
                {
                    //blocks til we get a new connection
                    TcpClient? client = Listener?.AcceptTcpClient();
                    if (client != null && client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint && remoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
                        AddClient(new AriesClient(this, client, secure, CN, email, WeakChainSignedRSAKey)
                        {
                            ADDR = remoteEndPoint.Address.MapToIPv4().ToString(),
                            SessionID = SessionID++
                        });
                    await Task.Delay(1);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                    && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"TCP DirtySock listener thrown a IOException! (IOException: {ex})");
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted 
                    && ex.SocketErrorCode != SocketError.ConnectionRefused && ex.SocketErrorCode != SocketError.Interrupted)
                    LoggerAccessor.LogError($"TCP DirtySock listener thrown a SocketException! (SocketException: {ex})");
            }
            catch (Exception ex)
            {
                if (ex.HResult != 995) LoggerAccessor.LogError($"TCP DirtySock listener thrown an assertion! (Exception: {ex})");
            }
        }

        public virtual void AddClient(AriesClient client)
        {
            lock (DirtySocksClients)
            {
                DirtySocksClients.Add(client);
            }
        }

        public virtual void RemoveClient(AriesClient client)
        {
            lock (DirtySocksClients)
            {
                DirtySocksClients.Remove(client);
            }
        }

        public void Broadcast(AbstractMessage msg)
        {
            lock (DirtySocksClients)
            {
                foreach (AriesClient? user in DirtySocksClients)
                {
                    user.PingSendTick = DateTime.Now.Ticks;
                    user.SendMessage(msg);
                }
            }
        }

        public virtual void HandleMessage(string name, byte[] data, AriesClient client)
        {
            try
            {
#if DEBUG
                LoggerAccessor.LogInfo($"{client.ADDR} Requested Type {name} : {{{DataTypesUtils.ByteArrayToHexString(data).Replace("\n", string.Empty)}}}");
#else
                LoggerAccessor.LogInfo($"{client.ADDR} Requested Type {name}");
#endif
                if (!NameToClass.TryGetValue(name, out Type? c))
                {
                    LoggerAccessor.LogError($"{client.ADDR} Requested an unexpected message Type {name}");
                    return;
                }

                AbstractMessage? msg = null;

                try
                {
                    msg = (AbstractMessage?)Activator.CreateInstance(c);
                }
                catch (Exception)
                {
                    // Not Important.
                }

                if (msg != null)
                {
                    msg.Read(Encoding.ASCII.GetString(data));
                    msg.Process(this, client);
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[AbstractDirtySockServer] - HandleMessage thrown an exception : {ex}");
            }
        }

        public void Dispose()
        {
            Listener?.Stop();
            Listener = null;
            ListenerThread.Join();
        }
    }
}
