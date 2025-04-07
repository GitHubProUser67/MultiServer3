using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    [RMCService(RMCProtocolId.RemoteLogDeviceService)]
	public class RemoteLogDeviceService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult Log(string strLine)
		{
			CustomLogger.LoggerAccessor.LogInfo($"[RMC] - Recieved from PID={Context?.Client?.PlayerInfo?.PID}: {strLine}");
			return Error(0);
		}
	}
}
