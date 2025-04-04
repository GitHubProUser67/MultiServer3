using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.GameServices.PCDriverServices
{
    [RMCService((ushort)RMCProtocolId.DriverUniqueIDService)]
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
