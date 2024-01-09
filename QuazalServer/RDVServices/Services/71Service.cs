using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
    [RMCService(RMCProtocolId._71Service)]

    public class _71Service : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult UKN1()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
