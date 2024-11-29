using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3DriverServices
{
    [RMCService(RMCProtocolId.OverlordCoreProtocolService)]
    public class OverlordCoreProtocolService : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult UKN1()
        {
            var list = new List<string>();
            return Result(list);
        }
    }
}
