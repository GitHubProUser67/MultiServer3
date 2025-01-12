using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.GameServices.PS3DriverServices
{
    [RMCService(RMCProtocolId.DriverUniqueIDService)]
    public class DriverUniqueIDService : RMCServiceBase
    {
        static uint UniqueIDCounter = 26434;

        [RMCMethod(2)]
        public RMCResult CreateUniqueID()
        {
            return Result(new { value = ++UniqueIDCounter });
        }
    }
}
