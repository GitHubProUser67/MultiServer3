using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CustomLogger;
using MultiSocks.Aries.Messages;
using MultiSocks.Aries.Model;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;
using MultiSocks.ProtoSSL;
using MultiSocks.ProtoSSL.Crypto;
using NetworkLibrary.Extension;

namespace MultiSocks.Aries
{
    public class AriesClient
    {
        public AbstractAriesServer Context;
        public AriesUser? User;
        public string ADDR = "127.0.0.1";
        public string LADDR = "127.0.0.1";
        public string VERS = string.Empty;
        public string SKU = string.Empty;
        public string SKEY = string.Empty;
        public string? Port = null;
        public int SessionID;
        public bool CanAsyncGameSearch = false;
        public bool CanAsync = true;

        private readonly bool secure;
        private readonly SemaphoreSlim dequeueSemaphore = new(1, 1);
        private readonly TcpClient tcpClient;
        private readonly Thread RecvThread;
        private readonly ConcurrentQueue<AbstractMessage> AsyncMessageQueue = new();
        private Stream? ClientStream;
        private string CommandName = "null";

        private (AsymmetricKeyParameter, Certificate, X509Certificate2) SecureKeyCert;

        public long PingSendTick;
        public int Ping;

        private const int MAX_SIZE = 1024 * 1024 * 2;

        public AriesClient(AbstractAriesServer context, TcpClient client, bool secure, string CN, bool WeakChainSignedRSAKey)
        {
            this.secure = secure;
            Context = context;
            tcpClient = client;

            LoggerAccessor.LogInfo("New connection from " + ADDR + ".");

            if (secure && context.SSLCache != null)
            {
                if (CN == "fesl.ea.com")
                    SecureKeyCert = context.SSLCache.GetVulnerableFeslEaCert(true);
                else
                    SecureKeyCert = context.SSLCache.GetVulnerableLegacyCustomEaCert(CN, WeakChainSignedRSAKey, true);
            }

            RecvThread = new Thread(RunLoop);
            RecvThread.Start();
        }

        private void RunLoop()
        {
            if (secure)
            {
                Ssl3TlsServer connTls = new(new Rc4TlsCrypto(false), SecureKeyCert.Item2, SecureKeyCert.Item1);
                TlsServerProtocol serverProtocol = new(tcpClient.GetStream());

                try
                {
                    serverProtocol.Accept(connTls);
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError($"[AriesClient] - ProtoSSL - Failed to accept connection:{e}");

                    serverProtocol.Flush();
                    serverProtocol.Close();
                    connTls.Cancel();

                    ClientStream?.Dispose();
                    tcpClient.Dispose();
                    LoggerAccessor.LogWarn($"[AriesClient] - User {ADDR} Disconnected.");
                    Context.RemoveClient(this);

                    return;
                }

                ClientStream = serverProtocol.Stream;
            }
            else
                ClientStream = tcpClient.GetStream();

            bool InHeader = false;
            int len, TempDatOff = 0;
            int ExpectedBytes = -1;
            Span<byte> TempData = null;
            Span<byte> bytes = new byte[65536];

            try
            {
                while (tcpClient.IsConnected())
                {
                    if (tcpClient.Available > 0)
                    {
                        len = ClientStream.Read(bytes);
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
                                bytes.Slice(off, copyLen).CopyTo(TempData.Slice(TempDatOff));
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
                                            tcpClient.Close(); // either something terrible happened or they're trying to mess with us
                                            break;
                                        }
                                        CommandName = Encoding.ASCII.GetString(TempData)[..4];

                                        TempData = new byte[size - 12];
                                        TempDatOff = 0;
                                    }
                                    else
                                    {
                                        // message complete, process in a sync manner to avoids issues.
                                        GotMessage(CommandName, TempData.ToArray());

                                        TempDatOff = 0;
                                        ExpectedBytes = -1;
                                        TempData = null;
                                    }
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
            tcpClient.Dispose();
            LoggerAccessor.LogWarn($"[AriesClient] - User {ADDR} Disconnected.");
            Context.RemoveClient(this);
        }

        private void GotMessage(string name, byte[] data)
        {
            Task.Run(() =>
            {
                Context.HandleMessage(name, data, this);
            }).Wait();
        }

        private Task DequeueAsyncMessage()
        {
            if (!dequeueSemaphore.Wait(0))
                return Task.CompletedTask;

            try
            {
                while (AsyncMessageQueue.TryDequeue(out AbstractMessage? msg))
                {
                    // Some games not like when async msgs are sent too close to each others (MOH).
                    Thread.Sleep(100);

                    if (msg != null)
                        SendImmediateMessage(msg.GetData());
                }
            }
            finally
            {
                dequeueSemaphore.Release();
            }

            return Task.CompletedTask;
        }

        public bool SendImmediateMessage(byte[] data)
        {
            if (ClientStream != null)
            {
                try
                {
                    ClientStream.Write(data);

                    return true;
                }
                catch
                {
                    // something bad happened :(
                }
            }

            return false;
        }

        public void SendMessage(AbstractMessage msg)
        {
            if (msg._Name.Equals("+gam") && !CanAsyncGameSearch)
                return;
            else if (msg._Name.StartsWith('+'))
            {
                if (CanAsync)
                    AsyncMessageQueue.Enqueue(msg);

                _ = DequeueAsyncMessage();

                return;
            }

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
