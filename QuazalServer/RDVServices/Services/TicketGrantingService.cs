using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;
using QuazalServer.RDVServices.Entities;
using System.Net;
using System.Security.Cryptography;
using System.Globalization;
using System.Text.RegularExpressions;

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
                Match iswii = new Regex(@"\(([^()]*)\)").Match(userName); // Check for the WII friend code.

                // create tracking client info
                PlayerInfo? plInfo = NetworkPlayers.GetPlayerInfoByUsername(userName);

                if (plInfo != null && plInfo.Client != null &&
                    !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                    (DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds < Constants.ClientTimeoutSeconds)
                {
                    CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName} was already logged-in - disconnecting...");
                    NetworkPlayers.DropPlayerInfo(plInfo);
                }

                CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName}");

                plInfo = NetworkPlayers.CreatePlayerInfo(Context.Client);

                User? user = DBHelper.GetUserByName(userName, Context.Handler.AccessKey);

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
                                    string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                    new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 }
                                    })
                                },
                                strReturnMsg = string.Empty,
                                pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, "JaDe!")
                            };
                        else
                            reply = new(plInfo.PID)
                            {
                                retVal = (int)ErrorCode.Core_NoError,
                                pConnectionData = new RVConnectionData()
                                {
                                    m_urlRegularProtocols = new(
                                    "prudps",
                                    string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                    new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 }
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
                                string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                new Dictionary<string, int>() {
                                        { "port", Context.Handler.BackendPort },
                                        { "CID", 1 },
                                        { "PID", (int)Context.Client.sPID },
                                        { "sid", 1 },
                                        { "stream", 3 },
                                        { "type", 2 }
                                })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey, File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{userName}_{plInfo.PID}_password.txt"))
                        };
                    else
                        return Error((int)ErrorCode.RendezVous_InvalidPassword);

                    return Result(reply);
                }
                else if (Context.Handler.AccessKey == "QusaPha9" || Context.Handler.AccessKey == "OLjNg84Gh" || Context.Handler.AccessKey == "ridfebb9") // Console login not uses Quazal storage, they use a given account to log-in.
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
                                            string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                            new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 }
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
                                            string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress,
                                            new Dictionary<string, int>() {
                                            { "port", Context.Handler.BackendPort },
                                            { "CID", 1 },
                                            { "PID", (int)Context.Client.sPID },
                                            { "sid", 1 },
                                            { "stream", 3 },
                                            { "type", 2 }
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

                    if (plInfo != null && plInfo.Client != null &&
                            !plInfo.Client.Endpoint.Equals(Context.Client.Endpoint) &&
                            (DateTime.UtcNow - plInfo.Client.LastPacketTime).TotalSeconds < Constants.ClientTimeoutSeconds)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName} was already logged-in - disconnecting...");
                        NetworkPlayers.DropPlayerInfo(plInfo);
                    }

                    User? user = DBHelper.GetUserByName(userName, Context.Handler.AccessKey);

                    if (user != null)
                    {
                        bool passwordCheckResult = false;
                        try
                        {
                            passwordCheckResult = oExtraData.data.password == user.Password || SecurePasswordHasher.Verify($"{user.Id}-{user.PlayerNickName}", oExtraData.data.password);
                        }
                        catch (Exception)
                        {
                            passwordCheckResult = false;
                        }

                        if (passwordCheckResult)
                            CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - User login request {userName} - success");
                        else
                        {
                            CustomLogger.LoggerAccessor.LogWarn($"[RMC Authentication] - User login request {userName} - invalid password");
                            loginCode = ErrorCode.RendezVous_InvalidPassword;
                        }
                    }
                    else
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
                                    { "port", Context.Handler.BackendPort },
                                    { "CID", 1 },
                                    { "PID", (int)Context.Client.sPID },
                                    { "sid", 1 },
                                    { "stream", 3 },
                                    { "type", 2 }
                                    })
                            },
                            strReturnMsg = string.Empty,
                            pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey)
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
                    ticketData.pbufResponse = kerberos.toBuffer(Context.Handler.AccessKey);
                else
                {
                    User? user = DBHelper.GetUserByPID(sourcePID, Context.Handler.AccessKey);

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

    public static class SecurePasswordHasher
    {
        private const int SaltSize = 8;
        private const int HashSize = 6;

        private const int HashIterations = 10000;

        private static string Hash(string password, int iterations)
        {
            // Create salt
            byte[] salt = Array.Empty<byte>();

            using (RNGCryptoServiceProvider rng = new())
            {
                rng.GetBytes(salt = new byte[SaltSize]);
            }

            // Create hash
            byte[] hash;
            using (Rfc2898DeriveBytes pbkdf2 = new(password, salt, iterations))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            // Combine salt and hash
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert to base64
            var base64Hash = Convert.ToHexString(hashBytes);

            // Format hash with extra information
            return base64Hash;
        }

        public static string Hash(string password)
        {
            return Hash(password, HashIterations);
        }

        public static bool Verify(string password, string base64Hash)
        {
            // Get hash bytes
            var hashBytes = Convert.FromHexString(base64Hash);

            // Get salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Create hash with given salt
            byte[] hash;
            using (Rfc2898DeriveBytes pbkdf2 = new(password, salt, HashIterations))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            // Get result
            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }
    }
}
