using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;
using QuazalServer.RDVServices.Entities;
using System.Net;
using System.Text.RegularExpressions;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// Authentication service (ticket granting)
	/// </summary>
	[RMCService(RMCProtocolId.TicketGrantingServiceLoginData)]
	public class TicketGrantingServiceLoginData : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult Login(string userName)
		{
			if (Context != null)
			{
                string prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress;
                if (QuazalServerConfiguration.UsePublicIP)
                    prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerPublicBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerPublicBindAddress;

                Match iswii = new Regex(@"\(([^()]*)\)").Match(userName); // Check for the WII friend code.

                // create tracking client info
                PlayerInfo? plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

                if (plInfo != null && plInfo.Client != null &&
                    !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                    plInfo.Client.TimeSinceLastPacket < Constants.ClientTimeoutSeconds)
                {
                    CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName} was already logged-in - disconnecting...");
                    NetworkPlayers.DropPlayerInfo(plInfo);
                }

                CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName}");

                plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

                var keypair = DBHelper.GetUserByName(userName, Context.Handler.AccessKey);

                User? user = keypair?.Item3;

                if (user != null || userName == "guest" || userName == "Tracking")
                {
                    if (user != null)
                    {
                        plInfo.PID = user.PID;
                        plInfo.AccountId = user.Username;
                        plInfo.Name = user.Name;
                    }
                    else
                    {
                        if (userName == "guest")
                            plInfo.PID = 100;
                        plInfo.AccountId = userName;
                        plInfo.Name = userName;
                    }

                    KerberosTicket kerberos = new(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket);

                    Login reply = new(0);

                    if (user == null)
                    {
                        if (userName == "Tracking")
                            reply = new(0)
                            {
                                retVal = (int)ErrorCode.Core_NoError,
                                pConnectionData = new RVConnectionData()
                                {
                                    m_urlRegularProtocols = new(
                                    "prudps",
                                    prudplink,
                                    new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 } // Public, not BehindNAT
                                    })
                                },
                                strReturnMsg = string.Empty,
                                pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "JaDe!")
                            };
                        else if (userName == "guest")
                            reply = new(plInfo.PID)
                            {
                                retVal = (int)ErrorCode.Core_NoError,
                                pConnectionData = new RVConnectionData()
                                {
                                    m_urlRegularProtocols = new(
                                    "prudps",
                                    prudplink,
                                    new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 } // Public, not BehindNAT
                                    })
                                },
                                strReturnMsg = string.Empty,
                                pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "h7fyctiuucf")
                            };
                        else
                            reply = new(plInfo.PID)
                            {
                                retVal = (int)ErrorCode.Core_NoError,
                                pConnectionData = new RVConnectionData()
                                {
                                    m_urlRegularProtocols = new(
                                    "prudps",
                                    prudplink,
                                    new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 } // Public, not BehindNAT
                                    })
                                },
                                strReturnMsg = string.Empty,
                                pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey)
                            };
                    }
                    else if (File.Exists(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{userName}_{plInfo.PID}_password.txt"))
                        reply = new(plInfo.PID)
                        {
                            retVal = (int)ErrorCode.Core_NoError,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new(
                                "prudps",
                                prudplink,
                                new Dictionary<string, int>() {
                                        { "port", Context.Handler.BackendPort },
                                        { "CID", 1 },
                                        { "PID", (int)Context.Client.sPID },
                                        { "sid", 1 },
                                        { "stream", 3 },
                                        { "type", 2 } // Public, not BehindNAT
                                })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{userName}_{plInfo.PID}_password.txt"))
                        };
                    else
                        return Error((int)ErrorCode.RendezVous_InvalidPassword);

                    return Result(reply);
                }
                else if (Context.Handler.AccessKey == "QusaPha9" || Context.Handler.AccessKey == "cYoqGd4f" 
                    || Context.Handler.AccessKey == "OLjNg84Gh" || Context.Handler.AccessKey == "ridfebb9" 
                    || Context.Handler.AccessKey == "q1UFc45UwoyI" || Context.Handler.AccessKey == "h0rszqTw"
                    || Context.Handler.AccessKey == "os4R9pEiy" || Context.Handler.AccessKey == "lON6yKGp"
                    || Context.Handler.AccessKey == "4TeVtJ7V" || Context.Handler.AccessKey == "HJb8Ix1M"
                     || Context.Handler.AccessKey == "uG9Kv3p") // Console login not uses Quazal storage, they use a given account to log-in.
                {
                    if (iswii.Success) // WII uses a master account.
                    {
                        string wiifc = iswii.Groups[1].Value;

                        Context.Client.WIIFriendCode = wiifc;

                        plInfo.PID = 50; // Arbitrary.
                        plInfo.AccountId = "Master User";
                        plInfo.Name = "Master User";

                        return Result(new Login(plInfo.PID)
                        {
                            retVal = (int)ErrorCode.Core_NoError,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new(
                                            "prudps",
                                            prudplink,
                                            new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 } // Public, not BehindNAT
                                            })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket).toBuffer(Context.Handler.AccessKey, wiifc)
                        });
                    }
                    else // PS and XBOX in theory.
                    {
                        plInfo.PID = NetworkPlayers.GenerateUniqueUint(userName + "a1nPut!");
                        plInfo.AccountId = userName;
                        plInfo.Name = userName;

                        return Result(new Login(plInfo.PID)
                        {
                            retVal = (int)ErrorCode.Core_NoError,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new(
                                            "prudps",
                                            prudplink,
                                            new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 } // Public, not BehindNAT
                                            })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket).toBuffer(Context.Handler.AccessKey)
                        });
                    }
                }
                else
                    return Error((int)ErrorCode.RendezVous_InvalidUsername);
            }

			return Error(0);
        }

		/// <summary>
		/// Function where client login is performed by account ID and password
		/// </summary>
		[RMCMethod(2)]
		public RMCResult LoginEx(string userName, AnyData<LoginData> oExtraData)
		{
			if (Context != null)
			{
                string prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress;
                if (QuazalServerConfiguration.UsePublicIP)
                    prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerPublicBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerPublicBindAddress;

                if (oExtraData.data != null)
                {
                    ErrorCode loginCode = ErrorCode.Core_NoError;

                    PlayerInfo? plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

                    if (plInfo != null && plInfo.Client != null &&
                            !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                            plInfo.Client.TimeSinceLastPacket < Constants.ClientTimeoutSeconds)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName} was already logged-in - disconnecting...");
                        NetworkPlayers.DropPlayerInfo(plInfo);
                    }

                    var keypair = DBHelper.GetUserByName(userName, Context.Handler.AccessKey);

                    User? user = keypair?.Item3;

                    if (user == null)
                    {
                        CustomLogger.LoggerAccessor.LogWarn($"[RMC Authentication] - User login request {userName} - invalid user name");
                        loginCode = ErrorCode.RendezVous_InvalidUsername;
                    }

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
                        Login loginData;

                        plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

                        if (user != null)
                        {
                            plInfo.PID = user.PID;
                            plInfo.AccountId = user.Username;
                            plInfo.Name = user.Name;
                        }
                        else
                        {
                            plInfo.AccountId = userName;
                            plInfo.Name = userName;
                        }

                        if (!File.Exists(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{userName}_{plInfo.PID}_password.txt"))
                        {
                            loginData = new(0)
                            {
                                retVal = (int)ErrorCode.RendezVous_InvalidPassword,
                                pConnectionData = new RVConnectionData()
                                {
                                    m_urlRegularProtocols = new StationURL("prudp:/")
                                },
                                strReturnMsg = string.Empty,
                                pbufResponse = Array.Empty<byte>()
                            };

                            return Result(loginData);
                        }

                        KerberosTicket kerberos = new(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.ticket);

                        loginData = new(plInfo.PID)
                        {
                            retVal = (uint)loginCode,
                            pConnectionData = new RVConnectionData()
                            {
                                m_urlRegularProtocols = new(
                                    "prudps",
                                    prudplink,
                                    new Dictionary<string, int>() {
                                        { "port", Context.Handler.BackendPort },
                                        { "CID", 1 },
                                        { "PID", (int)Context.Client.sPID },
                                        { "sid", 1 },
                                        { "stream", 3 },
                                        { "type", 2 } // Public, not BehindNAT
                                    })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{userName}_{plInfo.PID}_password.txt"))
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
            if (Context != null)
            {
                KerberosTicket kerberos = new(sourcePID, targetPID, Constants.SessionKey, Constants.ticket);

                TicketData ticketData = new()
                {
                    retVal = (int)ErrorCode.Core_NoError,
                };

                if (sourcePID == 0) // Ubisoft tracker account.
                    ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "JaDe!");
                else if (sourcePID == 50 && !string.IsNullOrEmpty(Context.Client.WIIFriendCode)) // WII Quazal account.
                    ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, Context.Client.WIIFriendCode);
                else if (sourcePID == 100) // Quazal guest account.
                    ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "h7fyctiuucf");
                else
                {
                    var keypair = DBHelper.GetUserByPID(sourcePID, Context.Handler.AccessKey);

                    User? user = keypair?.Item3;

                    if (user != null && File.Exists(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{user.Name}_{sourcePID}_password.txt"))
                        ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{user.Name}_{sourcePID}_password.txt"));
                    else
                        ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey);
                }

                return Result(ticketData);
            }

            return Error(0);
        }
    }
}
