
using CustomLogger;
using CyberBackendLibrary.DataTypes;
using MultiSocks.DirtySocks.Messages;
using MultiSocks.Tls;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiSocks.DirtySocks
{
    public abstract class AbstractDirtySockServer : IDisposable
    {
        public abstract Dictionary<string, Type> NameToClass { get; }
        public string? Project = null;
        public string? SKU = null;
        public bool lowlevel = false;
        public int SessionID = 1;
        public ProtoSSLUtils? SSLCache = null;
        public List<DirtySockClient> DirtySocksClients = new();
        public TcpListener? Listener;

        private bool secure = false;
        private bool WeakChainSignedRSAKey = false;
        private string CN = string.Empty;
        private string email = string.Empty;
        private Thread ListenerThread;

        public AbstractDirtySockServer(ushort port, bool lowlevel, string? Project = null, string? SKU = null, bool secure = false, string CN = "", string email = "", bool WeakChainSignedRSAKey = false)
        {
            this.secure = secure;
            this.lowlevel = lowlevel;
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
                        AddClient(new DirtySockClient(this, client, secure, CN, email, WeakChainSignedRSAKey)
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

        public virtual void AddClient(DirtySockClient client)
        {
            lock (DirtySocksClients)
            {
                DirtySocksClients.Add(client);
            }
        }

        public virtual void RemoveClient(DirtySockClient client)
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
                foreach (DirtySockClient? user in DirtySocksClients)
                {
                    user.PingSendTick = DateTime.Now.Ticks;
                    user.SendMessage(msg.GetData());
                }
            }
        }

        public virtual void HandleMessage(string name, byte[] data, DirtySockClient client)
        {
            try
            {
                string body = Encoding.ASCII.GetString(data);

                if (lowlevel) // Can be used a SSL workaround for testing.
                {
                    string hexdata = DataTypesUtils.ByteArrayToHexString(data);

                    LoggerAccessor.LogInfo($"{client.ADDR} Requested Packet {name}:{hexdata}:{{{body.Replace("\n", string.Empty)}}}");

                    switch (hexdata)
                    {
                        /*case "5243342B4D44352D563200": // BOP_PS3 RC4_MD5.
                            client.SendMessage(new MiscUtils().HexStringToByteArray("407469630000000000000060ba55778b9e10d44294388" +
                                "f79f770afe3cec0ddfffba532a61ff67726dc862f5104b224c1" +
                                "b76d7e1d649c57c7ae5071a1651b988d1baabfd3c3c77b4c0c0" +
                                "8c998e6ccd21cea00f94b90bdd38cd08838fd5d4506e2"));
                            return; Workaround is to not respond at all */
                        default: // Fallback to classic handler.
                            break;
                    }
                }
                else
                    LoggerAccessor.LogInfo($"{client.ADDR} Requested Type {name} : {body.Replace("\n", "")}");

                Type? c;
                if (!NameToClass.TryGetValue(name, out c))
                {
                    LoggerAccessor.LogError($"{client.ADDR} Requested an unexpected message Type {name} : {body.Replace("\n", "")}");
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
                    msg.Read(body);
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
