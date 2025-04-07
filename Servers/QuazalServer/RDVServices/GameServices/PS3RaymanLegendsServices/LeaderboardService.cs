using QuazalServer.QNetZ.Attributes;
using CustomLogger;
using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;

namespace QuazalServer.RDVServices.GameServices.PS3RaymanLegendsServices
{
    /// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService((ushort)RMCProtocolId.LeaderboardService)]
    public class LeaderboardService : RMCServiceBase
    {

        [RMCMethod(1)]
        public RMCResult GetList()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
