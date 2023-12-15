using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;
using System.Net;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// Authentication service (ticket granting)
	/// </summary>
	[RMCService(RMCProtocolId.TicketGrantingService)]
	public class TicketGrantingService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult Login(string userName)
		{
			if (Context != null)
			{
                // var trackingLoginData = "01 00 01 00 69 00 00 00 4C 00 00 00 99 39 C6 CB 93 13 50 8C 0B 02 C2 0B BC E4 94 6E B8 57 D0 15 A7 A1 AB 03 57 3F C1 69 F6 8E DC 55 0A A3 72 61 81 37 EB 6C A5 0C A2 C2 66 D5 B0 C6 23 15 E5 99 5A 3C 1F EC F7 90 55 2F 33 1E B7 C1 05 52 41 83 A0 1E 3F E8 18 02 7B 3B 4A 00 70 72 75 64 70 73 3A 2F 61 64 64 72 65 73 73 3D 31 38 35 2E 33 38 2E 32 31 2E 38 33 3B 70 6F 72 74 3D 32 31 30 30 36 3B 43 49 44 3D 31 3B 50 49 44 3D 32 3B 73 69 64 3D 31 3B 73 74 72 65 61 6D 3D 33 3B 74 79 70 65 3D 32 00 00 00 00 00 01 00 00 01 00 00";
                // 
                // var m = new MemoryStream(Helper.ParseByteArray(trackingLoginData));
                // 
                // var retModel = DDLSerializer.ReadObject<Login>(m);

                // create tracking client info
                PlayerInfo? plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

                if (plInfo != null && plInfo.Client != null &&
                    !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                    (DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds < Constants.ClientTimeoutSeconds)
                {
                    CustomLogger.LoggerAccessor.LogInfo($"User login request {userName} DENIED - concurrent login!");
                    return Error((int)ErrorCode.RendezVous_ConcurrentLoginDenied);
                }

                CustomLogger.LoggerAccessor.LogInfo($"User login request {userName}");

                // TODO - DB to implement.

                plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

                plInfo.PID = (uint)new Random().Next(); // THIS IS HORRIBLE REMOVE THAT.
                plInfo.AccountId = userName;
                plInfo.Name = userName;

                KerberosTicket kerberos = new(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket);

                Login reply = new(plInfo.PID)
                {
                    retVal = (int)ErrorCode.Core_NoError,
                    pConnectionData = new RVConnectionData()
                    {
                        m_urlRegularProtocols = new(
                            "prudps",
                            string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                            new Dictionary<string, int>() {
                            { "port", QuazalServerConfiguration.BackendServiceServerPort },
                            { "CID", 1 },
                            { "PID", (int)Context.Client.sPID },
                            { "sid", 1 },
                            { "stream", 3 },
                            { "type", 2 }
                            })
                    },
                    strReturnMsg = string.Empty,
                    pbufResponse = kerberos.toBuffer()
                };

                return Result(reply);
            }

			return Error(0);
        }

		/// <summary>
		/// Function where client login is performed by account ID and password
		/// </summary>
		/// <param name="login"></param>
		[RMCMethod(2)]
		public RMCResult LoginEx(string userName, AnyData<UbiAuthenticationLoginCustomData> oExtraData)
		{
			if (Context != null)
			{
                if (oExtraData.data != null)
                {
                    ErrorCode loginCode = ErrorCode.Core_NoError;

                    PlayerInfo? plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

                    if (plInfo != null)
                    {
                        if (plInfo.Client != null &&
                            !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                            (DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds < Constants.ClientTimeoutSeconds)
                        {
                            CustomLogger.LoggerAccessor.LogInfo($"User login request {userName} - concurrent login!");
                            loginCode = ErrorCode.RendezVous_ConcurrentLoginDenied;
                        }
                        else
                            NetworkPlayers.DropPlayerInfo(plInfo);
                    }

                    // TODO - DB to implement.

                    if (loginCode != ErrorCode.Core_NoError)
                    {
                        Login loginData = new(0)
                        {
                            retVal = (uint)loginCode,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new StationURL("prudp:/")
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = Array.Empty<byte>()
                        };

                        return Result(loginData);
                    }
                    else
                    {
                        plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

                        plInfo.PID = (uint)new Random().Next(); // THIS IS HORRIBLE REMOVE THAT.
                        plInfo.AccountId = userName;
                        plInfo.Name = oExtraData.data.username;

                        KerberosTicket kerberos = new(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket);

                        Login loginData = new(plInfo.PID)
                        {
                            retVal = (uint)loginCode,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new(
                                    "prudps",
                                    string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                    new Dictionary<string, int>() {
                                    { "port", QuazalServerConfiguration.BackendServiceServerPort },
                                    { "CID", 1 },
                                    { "PID", (int)Context.Client.sPID },
                                    { "sid", 1 },
                                    { "stream", 3 },
                                    { "type", 2 }
                                    })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = kerberos.toBuffer()
                        };

                        return Result(loginData);
                    }
                }
                else
                    CustomLogger.LoggerAccessor.LogError($"[RMC Authentication] - Unknown Custom Data class '{oExtraData.className}'");

                return Error((int)ErrorCode.RendezVous_ClassNotFound);
            }

            return Error(0);
        }

        [RMCMethod(3)]
		public RMCResult RequestTicket(uint sourcePID, uint targetPID)
		{
            KerberosTicket kerberos = new(sourcePID, targetPID, Constants.SessionKey, Constants.ticket);

            TicketData ticketData = new()
			{
				retVal = (int)ErrorCode.Core_NoError,
				pbufResponse = kerberos.toBuffer()
			};

			return Result(ticketData);
		}
	}
}
