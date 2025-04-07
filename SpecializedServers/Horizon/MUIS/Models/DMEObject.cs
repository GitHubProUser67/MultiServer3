using CustomLogger;
using Horizon.RT.Models;
using System.Net;


namespace Horizon.MUIS.Models
{
    public class DMEObject : ClientObject
    {
        public int MaxWorlds { get; protected set; } = 0;
        public int MaxPlayersPerWorld { get; protected set; } = 0;
        public int CurrentWorlds { get; protected set; } = 0;
        public int CurrentPlayers { get; protected set; } = 0;

        public int Port { get; protected set; } = 0;
        public new IPAddress IP { get; protected set; } = IPAddress.Any;

        public override bool Timedout => false; // (Utils.GetHighPrecisionUtcTime() - UtcLastEcho).TotalSeconds > MUISStarter.Settings.DmeTimeoutSeconds;
        public override bool IsConnected => _hasActiveSession && !Timedout;
        public override bool IsLoggedIn => _hasActiveSession;

        #region DMEObjects
        public DMEObject(ClientObject client, MediusServerCreateGameOnSelfRequest request)
        {
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;
            IP = client.IP;

            // Generate new session key
            SessionKey = MuisClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(ClientObject client, MediusServerCreateGameOnSelfRequest0 request)
        {
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;
            IP = client.IP;

            // Generate new session key
            SessionKey = MuisClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(ClientObject client, MediusServerCreateGameOnMeRequest request)
        {
            ApplicationId = request.ApplicationID;
            WorldId = request.WorldID;
            IP = client.IP;

            // Generate new session key
            SessionKey = MuisClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerSetAttributesRequest request)
        {
            Port = request.ListenServerAddress.Port;
            SetIp(request.ListenServerAddress.Address ?? MuisClass.SERVER_IP.ToString());

            // Generate new session key
            SessionKey = MuisClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            Token = Convert.ToBase64String(tokenBuf);
        }

        public DMEObject(MediusServerSessionBeginRequest request)
        {
            ApplicationId = request.ApplicationID;
            Port = request.Port;

            // Generate new session key
            SessionKey = MuisClass.GenerateSessionKey();

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
            SessionKey = MuisClass.GenerateSessionKey();

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
