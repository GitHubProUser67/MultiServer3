using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using System.Net;

namespace Horizon.MEDIUS.Medius.Models
{
    public class DMEObject : ClientObject
    {
        public int MaxWorlds { get; protected set; } = 0;
        public int MaxPlayersPerWorld { get; protected set; } = 0;
        public int CurrentWorlds { get; protected set; } = 0;
        public int CurrentPlayers { get; protected set; } = 0;

        public MGCL_ALERT_LEVEL MGCL_ALERT_LEVEL { get; protected set; } = MGCL_ALERT_LEVEL.MGCL_ALERT_NONE;
        public int Port { get; protected set; } = 0;
        public new int UdpPort { get; protected set; } = 50000;
        public new IPAddress IP { get; protected set; } = IPAddress.Any;

        public override bool Timedout => false; // (Utils.GetHighPrecisionUtcTime() - UtcLastEcho).TotalSeconds > Program.Settings.DmeTimeoutSeconds;
        public override bool IsConnected => _hasActiveSession && !Timedout;
        public override bool IsLoggedIn => _hasActiveSession;

        #region DMEObjects

        #region Peer to Peer
        public DMEObject(MediusServerCreateGameOnSelfRequest request)
        {
            string? IP = request.AddressList.AddressList?[0].Address;
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;
            if (!string.IsNullOrEmpty(IP))
                SetIp(IP);

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerCreateGameOnSelfRequest0 request)
        {
            string? IP = request.AddressList.AddressList?[0].Address;
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;
            if (!string.IsNullOrEmpty(IP))
                SetIp(IP);

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerCreateGameOnMeRequest request)
        {
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }
        #endregion
        public DMEObject(MediusServerSetAttributesRequest request)
        {
            Port = request.ListenServerAddress.Port;
            SetIp(request.ListenServerAddress.Address ?? "0.0.0.0");

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerSessionBeginRequest request, string IP)
        {
            ApplicationId = request.ApplicationID;
            Port = request.Port;
            SetIp(IP ?? "0.0.0.0");

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerSessionBeginRequest1 request)
        {
            ApplicationId = request.ApplicationID;
            Port = request.Port;

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerSessionBeginRequest2 request)
        {
            ApplicationId = request.ApplicationID;
            Port = request.Port;

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }
        #endregion

        public void OnServerReport(MediusServerReport report)
        {
            MaxWorlds = report.MaxWorlds;
            MaxPlayersPerWorld = report.MaxPlayersPerWorld;
            CurrentWorlds = report.ActiveWorldCount;
            CurrentPlayers = report.TotalActivePlayers;
            MGCL_ALERT_LEVEL = report.AlertLevel;
        }

        #region SetIP
        public new void SetIp(string ip)
        {
            switch (Uri.CheckHostName(ip))
            {
                case UriHostNameType.IPv4:
                    {
                        IP = IPAddress.Parse(ip);
                        break;
                    }
                case UriHostNameType.IPv6:
                    {
                        IP = IPAddress.Parse(ip);
                        break;
                    }
                case UriHostNameType.Dns:
                    {
                        IP = Dns.GetHostAddresses(ip).FirstOrDefault()?.MapToIPv4() ?? IPAddress.Any;
                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogError($"Unhandled UriHostNameType {Uri.CheckHostName(ip)} from {ip} in DMEObject.SetIp()");
                        break;
                    }
            }
        }
        #endregion

        protected override void PostStatus()
        {
            // Don't post stats of DME servers to db
        }
    }
}
