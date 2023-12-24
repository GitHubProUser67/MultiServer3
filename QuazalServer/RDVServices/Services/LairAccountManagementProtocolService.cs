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
            // We need to send the EDNET ip in hex endian swaped.

            if (AmhLairProxy.TryConvertIpAddressToHex(QuazalServerConfiguration.EdNetBindAddress, out uint result))
                return Result(new { retVal = result });
            else
                return Result(new { retVal = result });
        }
    }
}
