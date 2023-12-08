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
		private byte[] ticket = new byte[] {
			0x76, 0x21, 0x4B, 0xA6, 0x21, 0x96, 0xD3, 0xF3, 0x9A,
			0x8C, 0x7A, 0x27, 0x0D, 0xD9, 0xB3, 0xFA, 0x21, 0x0E,
			0xED, 0xAF, 0x42, 0x63, 0x92, 0x95, 0xC1, 0x16, 0x54,
			0x08, 0xEE, 0x6E, 0x69, 0x17, 0x35, 0x78, 0x2E, 0x6E
		};

		[RMCMethod(1)]
		public RMCResult Login(string userName)
		{
			var hostAddress = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress;

			return Error(0);
		}

		/// <summary>
		/// Function where client login is performed by account ID and password
		/// </summary>
		/// <param name="login"></param>
		[RMCMethod(2)]
		public RMCResult LoginEx(string userName, AnyData<UbiAuthenticationLoginCustomData> oExtraData)
		{
			var hostAddress = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress;

			var rdvConnectionString = new StationURL(
				"prudps",
				hostAddress,
				new Dictionary<string, int>() {
					{ "port", QuazalServerConfiguration.BackendServiceServerPort },
					{ "CID", 1 },
					{ "PID", (int)Context.Client.sPID },
					{ "sid", 1 },
					{ "stream", 3 },
					{ "type", 2 }
				});

			if (oExtraData.data != null)
			{
				ErrorCode loginCode = ErrorCode.Core_NoError;

				var plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

				if(plInfo != null)
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
					var loginData = new Login(0)
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
                    Random random = new(); // THIS IS HORRIBLE REMOVE THAT.

                    plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

					plInfo.PID = (uint)random.Next(); // THIS IS HORRIBLE REMOVE THAT.
					plInfo.AccountId = userName;
					plInfo.Name = oExtraData.data.username;

					var kerberos = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, ticket);

					var loginData = new Login(plInfo.PID)
					{
						retVal = (uint)loginCode,
						pConnectionData = new RVConnectionData()
						{
							m_urlRegularProtocols = rdvConnectionString
						},
						strReturnMsg = string.Empty,
						pbufResponse = kerberos.toBuffer()
					};

					return Result(loginData);
				}
			}
			else
                CustomLogger.LoggerAccessor.LogError($"[RMC Authentication] Error: Unknown Custom Data class '{oExtraData.className}'");

            return Error((int)ErrorCode.RendezVous_ClassNotFound);
		}

		[RMCMethod(3)]
		public RMCResult RequestTicket(uint sourcePID, uint targetPID)
		{
			var kerberos = new KerberosTicket(sourcePID, targetPID, Constants.SessionKey, ticket);

            TicketData ticketData = new()
			{
				retVal = (int)ErrorCode.Core_NoError,
				pbufResponse = kerberos.toBuffer()
			};

			return Result(ticketData);
		}
	}
}
