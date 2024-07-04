using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    [RMCService(RMCProtocolId._83Service)]
    public class _83Service : RMCServiceBase
    {
        [RMCMethod(2)]
        public RMCResult UKN2()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
