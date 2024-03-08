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
            // We need to send the EDNET ip in hex little endian.

            string destip = QuazalServerConfiguration.ServerBindAddress;
            if (!string.IsNullOrEmpty(QuazalServerConfiguration.EdNetBindAddressOverride))
                destip = QuazalServerConfiguration.EdNetBindAddressOverride;
            else if (QuazalServerConfiguration.UsePublicIP)
                destip = QuazalServerConfiguration.ServerPublicBindAddress;

            if (AmhLairProxy.TryConvertIpAddressToHex(destip, out uint result))
                return Result(new { retVal = result });
            else
                return Result(new { retVal = result });
        }
    }
}
