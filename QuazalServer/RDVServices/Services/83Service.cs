using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
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
