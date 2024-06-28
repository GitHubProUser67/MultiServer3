using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using CustomLogger;
using MultiSocks.Aries.SDK_v6.Messages;
using MultiSocks.Aries.SDK_v6.Model;
using MultiSocks.Tls;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;

namespace MultiSocks.Aries.SDK_v6
{
    public class AriesClient
    {
        public AbstractAriesServerV6 Context;
        public User? User;
        public string ADDR = "127.0.0.1";
        public string LADDR = "127.0.0.1";
        public string VERS = string.Empty;
        public string SKU = string.Empty;
        public string SKEY = string.Empty;
        public string? Port = null;
        public int SessionID;
        public bool CanAsyncGameSearch = false;
        public bool CanAsync = true;

        private int ExpectedBytes = -1;
        private bool InHeader;
        private bool secure;
        private TcpClient ClientTcp;
        private Stream? ClientStream;
        private Thread RecvThread;
        private ConcurrentQueue<AbstractMessage> AsyncMessageQueue = new();
        private byte[]? TempData = null;
        private int TempDatOff;
        private string CommandName = "null";

        private (AsymmetricKeyParameter, Certificate) SecureKeyCert;

        public long PingSendTick;
        public int Ping;

        private static int MAX_SIZE = 1024 * 1024 * 2;

        public AriesClient(AbstractAriesServerV6 context, TcpClient client, bool secure, string CN, string email, bool WeakChainSignedRSAKey)
        {
            this.secure = secure;
            Context = context;
            ClientTcp = client;

            LoggerAccessor.LogInfo("[Aries] - New connection from " + ADDR + ".");

            if (secure && context.SSLCache != null)
                SecureKeyCert = context.SSLCache.GetVulnerableLegacyCustomEaCert(CN, email, WeakChainSignedRSAKey);

            RecvThread = new Thread(RunLoop);
            RecvThread.Start();
        }

        public void Close()
        {
            ClientTcp.Close();
        }

        private void RunLoop()
        {
            if (secure)
            {
                NetworkStream? networkStream = ClientTcp.GetStream();

                Ssl3TlsServer connTls = new(new Rc4TlsCrypto(false), SecureKeyCert.Item2, SecureKeyCert.Item1);
                TlsServerProtocol serverProtocol = new(networkStream);

                try
                {
                    serverProtocol.Accept(connTls);
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError($"[Aries] - Failed to accept secure connection:{e}");

                    serverProtocol.Flush();
                    serverProtocol.Close();
                    connTls.Cancel();

                    ClientStream?.Dispose();
                    ClientTcp.Dispose();
                    LoggerAccessor.LogInfo($"[Aries] - User {ADDR} Disconnected.");
                    Context.RemoveClient(this);

                    return;
                }

                ClientStream = serverProtocol.Stream;
            }
            else
                ClientStream = ClientTcp.GetStream();

            int len = 0;
            Span<byte> bytes = new byte[65536];

            try
            {
                while ((len = ClientStream.Read(bytes)) != 0)
                {
                    int off = 0;
                    while (len > 0)
                    {
                        // got some data
                        if (ExpectedBytes == -1)
                        {
                            // new packet
                            InHeader = true;
                            ExpectedBytes = 12; // header
                            TempData = new byte[12];
                            TempDatOff = 0;
                        }

                        if (TempData != null)
                        {
                            int copyLen = Math.Min(len, TempData.Length - TempDatOff);
                            Array.Copy(bytes.ToArray(), off, TempData, TempDatOff, copyLen);
                            off += copyLen;
                            TempDatOff += copyLen;
                            len -= copyLen;

                            if (TempDatOff == TempData.Length)
                            {
                                if (InHeader)
                                {
                                    //header complete.
                                    InHeader = false;
                                    int size = TempData[11] | TempData[10] << 8 | TempData[9] << 16 | TempData[8] << 24;
                                    if (size > MAX_SIZE)
                                    {
                                        ClientTcp.Close(); // either something terrible happened or they're trying to mess with us
                                        break;
                                    }
                                    CommandName = Encoding.ASCII.GetString(TempData)[..4];

                                    TempData = new byte[size - 12];
                                    TempDatOff = 0;
                                }
                                else
                                {
                                    // message complete, process in a sync manner.
                                    Context.HandleMessage(CommandName, TempData, this);

                                    TempDatOff = 0;
                                    ExpectedBytes = -1;
                                    TempData = null;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            ClientStream?.Dispose();
            ClientTcp.Dispose();
            LoggerAccessor.LogInfo($"[Aries] - User {ADDR} Disconnected.");
            Context.RemoveClient(this);
        }

        public void EnqueueAsyncMessage(AbstractMessage msg)
        {
            AsyncMessageQueue.Enqueue(msg);
        }

        public void DequeueAsyncMessage()
        {
            while (AsyncMessageQueue.TryDequeue(out AbstractMessage? msg))
            {
                if (msg != null)
                {
                    SendAsyncMessage(msg);
                }
            }
        }

        public void SendMessage(AbstractMessage msg)
        {
            if (msg._Name.Equals("+gam") && !CanAsyncGameSearch)
                return;
            else if (msg._Name.StartsWith('+') && !CanAsync)
                return;

            try
            {
                ClientStream?.Write(msg.GetData());
            }
            catch
            {
                // something bad happened :(
            }

            DequeueAsyncMessage();
        }

        public void SendAsyncMessage(AbstractMessage msg)
        {
            if (msg._Name.Equals("+gam") && !CanAsyncGameSearch)
                return;
            else if (msg._Name.StartsWith('+') && !CanAsync)
                return;

            try
            {
                ClientStream?.Write(msg.GetData());
            }
            catch
            {
                // something bad happened :(
            }
        }

        public bool HasAuth()
        {
            return User != null;
        }
    }
}
