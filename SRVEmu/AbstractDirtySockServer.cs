using BackendProject;
using SRVEmu.Messages;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SRVEmu
{
    public abstract class AbstractDirtySockServer : IDisposable
    {
        public abstract Dictionary<string, Type> NameToClass { get; }
        public bool lowlevel = false;
        public int SessionID = 1;

        public List<DirtySockClient> DirtySocksClients = new();
        public TcpListener Listener;

        private Thread ListenerThread;

        public AbstractDirtySockServer(ushort port, bool lowlevel)
        {
            this.lowlevel = lowlevel;
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
                    TcpClient client = Listener.AcceptTcpClient();
                    if (client != null)
                    {
                        AddClient(new DirtySockClient(this, client)
                        {
                            SessionID = SessionID++
                        });
                    }
                    await Task.Delay(1);
                }
            }
            catch
            {
                CustomLogger.LoggerAccessor.LogWarn("TCP DirtySock listener stopped working!");
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
                    string hexdata = MiscUtils.ByteArrayToHexString(data);

                    CustomLogger.LoggerAccessor.LogInfo($"{client.IP} Requested Packet {name}:{hexdata}:{{{body.Replace("\n", "")}}}");

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
                    CustomLogger.LoggerAccessor.LogInfo($"{client.IP} Requested Type {name}:{body.Replace("\n", "")}");

                Type? c;
                if (!NameToClass.TryGetValue(name, out c))
                {
                    CustomLogger.LoggerAccessor.LogError($"{client.IP} Requested an unexpected message Type {name}:{body.Replace("\n", "")}");
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
                CustomLogger.LoggerAccessor.LogError($"[AbstractDirtySockServer] - HandleMessage thrown an exception : {ex}");
            }
        }

        public void Dispose()
        {
            
        }
    }
}
