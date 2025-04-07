using QuazalServer.QNetZ.Attributes;
using CustomLogger;
using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.QNetZ.Connection;

namespace QuazalServer.RDVServices.GameServices.PS3OFPS2Services
{
    /// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService((ushort)RMCProtocolId.SparkProtocolService)]
    public class SparkProtocolService : RMCServiceBase
    {

        [RMCMethod(4)]
        public RMCResult CreateGame()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(5)]
        public RMCResult JoinGame()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(7)]
        public RMCResult GetSelfStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(8)]
        public RMCResult GetParticipationStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(13)]
        public RMCResult GetPlayerStatus()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(13)]
        public RMCResult GetSecretQuestion()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(21)]
        public RMCResult QuickMatchWithHostUrls()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
