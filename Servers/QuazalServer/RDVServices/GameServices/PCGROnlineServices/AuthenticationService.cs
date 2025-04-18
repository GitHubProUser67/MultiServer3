using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Connection;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.RDVServices.RMC;
using RDVServices;
using System;
using System.Net;

namespace QuazalServer.RDVServices.GameServices.PCGROnlineServices
{
    [RMCService((ushort)RMCProtocolId.AuthenticationService)]
    public class AuthenticationService : RMCServiceBase
    {
        [RMCMethod(2)]
        public RMCResult LoginCustomData(string userName)
        {
            CustomLogger.LoggerAccessor.LogInfo($"[RMC Authentication] - TEST");

            if (Context != null)
            {
                string prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerBindAddress;
                if (QuazalServerConfiguration.UsePublicIP)
                    prudplink = string.IsNullOrWhiteSpace(QuazalServerConfiguration.ServerPublicBindAddress) ? Dns.GetHostName() : QuazalServerConfiguration.ServerPublicBindAddress;

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

                if (userName == "guest")
                {
                    plInfo.PID = 100;
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
                        pbufResponse = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.TicketData).ToBuffer(Context.Handler.AccessKey, "h7fyctiuucf")
                    });
                }
                else if (userName == "Tracking")
                {
                    plInfo.AccountId = userName;
                    plInfo.Name = userName;

                    return Result(new Login(0)
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
                        pbufResponse = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.TicketData).ToBuffer(Context.Handler.AccessKey, "JaDe!")
                    });
                }
                else // Console login not uses Quazal storage, they use a given account to log-in.
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
                        pbufResponse = new KerberosTicket(plInfo.PID, Context.Client.sPID, Constants.SessionKey, Constants.TicketData).ToBuffer(Context.Handler.AccessKey)
                    });
                }
            }

            /*
            CustomLogger.LoggerAccessor.LogInfo($"[RMC] - Request {context.RMC.request}");
            switch (context.RMC.request?.ToString())
            {

                case "UbiAuthenticationLoginCustomData": {

                        return Error(0);
                    }
            }
            */

            return Error(0);
        }


        [RMCMethod(3)]
        public RMCResult RequestTicket(uint sourcePID, uint targetPID)
        {
            if (Context != null)
            {
                KerberosTicket kerberos = new(sourcePID, targetPID, Constants.SessionKey, Constants.TicketData);

                TicketData ticketData = new()
                {
                    retVal = (int)ErrorCode.Core_NoError,
                };

                if (sourcePID == 0) // Ubisoft tracker account.
                    ticketData.pbufResponse = kerberos.ToBuffer(Context.Handler.AccessKey, "JaDe!");
                else if (sourcePID == 100) // Quazal guest account.
                    ticketData.pbufResponse = kerberos.ToBuffer(Context.Handler.AccessKey, "h7fyctiuucf");
                else
                    ticketData.pbufResponse = kerberos.ToBuffer(Context.Handler.AccessKey);

                return Result(ticketData);
            }
            return Error(0);
        }
    }
}