using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
    [RMCService(RMCProtocolId.LairAccountManagementProtocol)]
    public class LairAccountManagementProtocolService : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult AmhLairLogin()
        {
            return Result(new { retVal = 0x10001 });
        }
    }
}
