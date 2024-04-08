using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
    // Used in Rayman Legends PS3.
    [RMCService(RMCProtocolId._200Service)]
    public class _200Service : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult UKN1()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
