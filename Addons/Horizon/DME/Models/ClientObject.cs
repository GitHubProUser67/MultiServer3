using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.LIBRARY.Common;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.LIBRARY.Pipeline.Udp;
using System.Collections.Concurrent;
using System.Net;

namespace MultiServer.Addons.Horizon.DME.Models
{
    public class ClientObject
    {
        protected static Random RNG = new Random();

        /// <summary>
        /// 
        /// </summary>
        public UdpServer Udp { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public int UdpPort => Udp?.Port ?? -1;

        /// <summary>
        /// 
        /// </summary>
        public IChannel Tcp { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public int DmeId { get; protected set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public World DmeWorld { get; protected set; } = null;

        /// <summary>
        /// Current access token required to access the account.
        /// </summary>
        public string Token { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public string SessionKey { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public int ApplicationId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int? MediusVersion { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public uint ScertId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public RT_RECV_FLAG RecvFlag { get; set; } = RT_RECV_FLAG.RECV_SINGLE | RT_RECV_FLAG.RECV_LIST;

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<BaseScertMessage> TcpSendMessageQueue { get; } = new ConcurrentQueue<BaseScertMessage>();

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<ScertDatagramPacket> UdpSendMessageQueue { get; } = new ConcurrentQueue<ScertDatagramPacket>();

        /// <summary>
        /// 
        /// </summary>
        public DateTime UtcLastServerEchoSent { get; set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// 
        /// </summary>
        public DateTime UtcLastServerEchoReply { get; protected set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// RTT (ms)
        /// </summary>
        public uint LatencyMs { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeCreated { get; protected set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeAuthenticated { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public bool Disconnected { get; protected set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public IPEndPoint RemoteUdpEndpoint { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public int AggTimeMs { get; set; } = 20;

        /// <summary>
        /// 
        /// </summary>
        long? LastAggTime { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public bool HasJoined { get; set; } = false;

        public virtual bool IsConnectingGracePeriod => !TimeAuthenticated.HasValue && (Utils.GetHighPrecisionUtcTime() - TimeCreated).TotalSeconds < DmeClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds;
        public virtual bool Timedout => !IsConnectingGracePeriod && ((Utils.GetHighPrecisionUtcTime() - UtcLastServerEchoReply).TotalSeconds > DmeClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds);
        public virtual bool IsConnected => !Disconnected && !Timedout && Tcp != null && Tcp.IsActive;
        public virtual bool IsAuthenticated => TimeAuthenticated.HasValue;
        public virtual bool Destroy => Disconnected || (!IsConnected && !IsConnectingGracePeriod);
        public virtual bool IsDestroyed { get; protected set; } = false;
        public virtual bool IsAggTime => !LastAggTime.HasValue || (Utils.GetMillisecondsSinceStartup() - LastAggTime.Value) >= AggTimeMs;

        public Action<ClientObject> OnDestroyed;


        private DateTime _lastServerEchoValue = DateTime.UnixEpoch;

        public ClientObject(string sessionKey, World dmeWorld, int dmeId)
        {
            SessionKey = sessionKey;

            DmeId = dmeId;
            DmeWorld = dmeWorld;
            AggTimeMs = DmeClass.GetAppSettingsOrDefault(ApplicationId).DefaultClientWorldAggTime;

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public void BeginUdp()
        {
            if (Udp != null)
                return;

            Udp = new UdpServer(this);
            _ = Udp.Start();
        }

        public void QueueServerEcho()
        {
            TcpSendMessageQueue.Enqueue(new RT_MSG_SERVER_ECHO());
            UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime();
        }

        public void OnRecvServerEcho(RT_MSG_SERVER_ECHO echo)
        {
            var echoTime = echo.UnixTimestamp.ToUtcDateTime();
            if (echoTime > _lastServerEchoValue)
            {
                _lastServerEchoValue = echoTime;
                UtcLastServerEchoReply = Utils.GetHighPrecisionUtcTime();
                LatencyMs = (uint)(UtcLastServerEchoReply - echoTime).TotalMilliseconds;
            }
        }

        public void OnRecvClientEcho(RT_MSG_CLIENT_ECHO echo)
        {
            // older medius doesn't use server echo
            // so instead we'll increment our timeout dates by the client echo
            if (MediusVersion <= 108)
            {
                // reply must be before sent for the timeout to work
                UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime().AddSeconds(1);
                UtcLastServerEchoReply = Utils.GetHighPrecisionUtcTime();
            }
        }

        public Task HandleIncomingMessages()
        {
            // udp
            if (Udp != null)
                return Udp.HandleIncomingMessages();

            return Task.CompletedTask;
        }

        public void HandleOutgoingMessages()
        {
            List<BaseScertMessage> responses = new List<BaseScertMessage>();

            // set aggtime to locked intervals of whatever is stored in AggTimeMs
            // sometimes this server will be +- a few milliseconds on an agg and
            // we don't want that to change when messages get sent
            //if (LastAggTime.HasValue)
            //    LastAggTime += AggTimeMs * ((Utils.GetMillisecondsSinceStartup() - LastAggTime.Value) / AggTimeMs);
            //else
            LastAggTime = Utils.GetMillisecondsSinceStartup();

            // tcp
            if (Tcp != null)
            {
                while (TcpSendMessageQueue.TryDequeue(out var message))
                    responses.Add(message);

                // send
                if (responses.Count > 0)
                    _ = Tcp.WriteAndFlushAsync(responses);
            }

            // udp
            Udp?.HandleOutgoingMessages();
        }

        #region Connection / Disconnection

        public async Task Stop()
        {
            if (IsDestroyed)
                return;

            try
            {
                if (Udp != null)
                    await Udp.Stop();

                if (Tcp != null)
                    await Tcp.CloseAsync();
            }
            catch (Exception)
            {

            }
            finally
            {
                OnDestroyed?.Invoke(this);
            }

            Tcp = null;
            Udp = null;
            IsDestroyed = true;
        }

        public void OnTcpConnected(IChannel channel)
        {
            Tcp = channel;
        }

        public void OnTcpDisconnected()
        {
            Disconnected = true;
        }

        public void OnUdpConnected()
        {

        }

        public void OnConnectionCompleted()
        {
            TimeAuthenticated = Utils.GetHighPrecisionUtcTime();
        }

        #endregion

        #region Send Queue

        public void EnqueueTcp(BaseScertMessage message)
        {
            TcpSendMessageQueue.Enqueue(message);
        }

        public void EnqueueTcp(IEnumerable<BaseScertMessage> messages)
        {
            foreach (var message in messages)
                EnqueueTcp(message);
        }

        public void EnqueueTcp(IEnumerable<BaseMediusMessage> messages)
        {
            EnqueueTcp(messages.Select(x => new RT_MSG_SERVER_APP() { Message = x }));
        }

        public void EnqueueUdp(BaseScertMessage message)
        {
            Udp?.Send(message);
        }

        public void EnqueueUdp(IEnumerable<BaseScertMessage> messages)
        {
            foreach (var message in messages)
                EnqueueUdp(message);
        }

        public void EnqueueUdp(BaseMediusMessage message)
        {
            EnqueueUdp(new RT_MSG_SERVER_APP() { Message = message });
        }

        public void EnqueueUdp(IEnumerable<BaseMediusMessage> messages)
        {
            EnqueueUdp(messages.Select(x => new RT_MSG_SERVER_APP() { Message = x }));
        }

        #endregion

        public bool HasRecvFlag(RT_RECV_FLAG flag)
        {
            if (MediusVersion <= 108)
                return true;

            return RecvFlag.HasFlag(flag);
        }

        public override string ToString()
        {
            return $"(worldId: {DmeWorld.WorldId}, clientId: {DmeId})";
        }
    }
}
