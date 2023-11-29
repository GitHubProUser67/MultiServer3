using SRVEmu.Messages;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SRVEmu
{
    public abstract class AbstractDirtySockServer : IDisposable
    {
        public abstract Dictionary<string, Type> NameToClass { get; }
        public int SessionID = 1;

        public List<DirtySockClient> SrvEmuClients = new();
        public TcpListener Listener;

        private Thread ListenerThread;

        public AbstractDirtySockServer(ushort port)
        {
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
                        DirtySockClient eaC = new(this, client)
                        {
                            SessionID = SessionID++
                        };
                        AddClient(eaC);
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
            lock (SrvEmuClients)
            {
                SrvEmuClients.Add(client);
            }
        }

        public virtual void RemoveClient(DirtySockClient client)
        {
            lock (SrvEmuClients)
            {
                SrvEmuClients.Remove(client);
            }
        }

        public void Broadcast(AbstractMessage msg)
        {
            byte[] data = msg.GetData();
            lock (SrvEmuClients)
            {
                foreach (var user in SrvEmuClients)
                {
                    user.PingSendTick = DateTime.Now.Ticks;
                    user.SendMessage(data);
                }
            }
        }

        public virtual void HandleMessage(string name, byte[] data, DirtySockClient client)
        {
            try
            {
                string body = Encoding.ASCII.GetString(data);

                Type? c;
                if (!NameToClass.TryGetValue(name, out c))
                {
                    CustomLogger.LoggerAccessor.LogError($"{client.IP} Requested an unexpected message Type {name}:{body.Replace("\n", "")}");
                    return;
                }
                else
                    CustomLogger.LoggerAccessor.LogInfo($"{client.IP} Requested Type {name}:{body.Replace("\n", "")}");

                AbstractMessage? msg = (AbstractMessage?)Activator.CreateInstance(c);

                if (msg != null)
                {
                    msg.Read(body);
                    msg.Process(this, client);
                }
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            
        }
    }
}
