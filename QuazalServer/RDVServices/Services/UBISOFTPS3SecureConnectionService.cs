﻿using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;

namespace QuazalServer.RDVServices.Services
{
    /// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService(RMCProtocolId.UBISOFTPS3SecureConnectionService)]
    public class UBISOFTPS3SecureConnectionService : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult? Register(List<string> vecMyURLs)
        {
            if (Context != null && Context.Client.Info != null)
            {
                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString());
                rdvConnectionUrl.Address = Context.Client.Endpoint.Address.ToString();
                rdvConnectionUrl["type"] = 3;

                RegisterResult result = new()
                {
                    pidConnectionID = Context.Client.Info.RVCID,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                };

                return Result(result);
            }

            return null;
        }

        [RMCMethod(2)]
        public void RequestConnectionData()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(3)]
        public void RequestUrls()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(4)]
        public RMCResult RegisterEx(ICollection<StationURL> vecMyURLs, AnyData<SonyNPTicket> hCustomData)
        {
            if (hCustomData.data != null && Context != null && Context.Client.Info != null)
            {
                // change address
                StationURL rdvConnectionUrl = new(vecMyURLs.Last().ToString());
                rdvConnectionUrl.Address = Context.Client.Endpoint.Address.ToString();
                rdvConnectionUrl["type"] = 3;

                RegisterResult result = new()
                {
                    pidConnectionID = Context.Client.Info.RVCID,
                    retval = (int)ErrorCode.Core_NoError,
                    urlPublic = rdvConnectionUrl
                };

                return Result(result);
            }
            else
                CustomLogger.LoggerAccessor.LogInfo($"[RMC Secure] Error: Unknown Custom Data class {hCustomData.className}");

            return Error((int)ErrorCode.RendezVous_ClassNotFound);
        }

        [RMCMethod(5)]
        public void TestConnectivity()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(6)]
        public void UpdateURLs()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(7)]
        public void ReplaceURL()
        {
            UNIMPLEMENTED();
        }

        [RMCMethod(8)]
        public void SendReport()
        {
            UNIMPLEMENTED();
        }
    }
}
