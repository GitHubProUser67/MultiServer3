using System.Net;
using System.Net.Sockets;
using System.Text;
using CustomLogger;
using MultiSocks.DirtySocks.Messages;
using MultiSocks.DirtySocks.Model;
using MultiSocks.Tls;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Tls;

namespace MultiSocks.DirtySocks
{
    public class DirtySockClient
    {
        public AbstractDirtySockServer Context;
        public User? User;
        public string IP = "127.0.0.1";
        public string? Port = null;
        public int SessionID;

        private int ExpectedBytes = -1;
        private bool InHeader;
        private bool secure;
        private TcpClient ClientTcp;
        private Stream? ClientStream;
        private Thread RecvThread;
        private byte[]? TempData = null;
        private int TempDatOff;
        private string CommandName = "null";

        private (AsymmetricKeyParameter, Certificate) SecureKeyCert;

        public long PingSendTick;
        public int Ping;

        private static int MAX_SIZE = 1024 * 1024 * 2;

        public DirtySockClient(AbstractDirtySockServer context, TcpClient client, bool secure, string CN, string email, bool WeakChainSignedRSAKey)
        {
            this.secure = secure;
            Context = context;
            ClientTcp = client;
            IP = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() ?? "127.0.0.1";

            LoggerAccessor.LogInfo("New connection from " + IP + ".");

            if (secure)
                SecureKeyCert = new ProtoSSLUtils().GetVulnerableLegacyCustomEaCert(CN, email, WeakChainSignedRSAKey);

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
                    LoggerAccessor.LogError($"[DirtySocks ProtoSSL] - Failed to accept connection:{e}");

                    serverProtocol.Flush();
                    serverProtocol.Close();
                    connTls.Cancel();

                    ClientStream?.Dispose();
                    ClientTcp.Dispose();
                    LoggerAccessor.LogInfo($"User {IP} Disconnected.");
                    Context.RemoveClient(this);

                    return;
                }

                ClientStream = serverProtocol.Stream;
            }
            else
                ClientStream = ClientTcp.GetStream();

            int len = 0;
            byte[] bytes = new byte[65536];

            try
            {
                while ((len = ClientStream.Read(bytes, 0, bytes.Length)) != 0)
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
                            Array.Copy(bytes, off, TempData, TempDatOff, copyLen);
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
                                    // message complete
                                    GotMessage(CommandName, TempData);

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
            LoggerAccessor.LogInfo($"User {IP} Disconnected.");
            Context.RemoveClient(this);
        }

        private void GotMessage(string name, byte[] data)
        {
            Task.Run(() =>
            {
                Context.HandleMessage(name, data, this);
            });
        }

        public void SendMessage(byte[] data)
        {
            try
            {
                ClientStream?.Write(data);
            }
            catch
            {
                // something bad happened :(
            }
        }

        public void SendMessage(AbstractMessage msg)
        {
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
